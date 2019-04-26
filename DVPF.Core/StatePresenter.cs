namespace DVPF.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    // TODO: REFACTOR - Use Stylecop to enforce a maximum number of characters per line

    /// <summary>
    /// Presents an instance of <seealso cref="State"/> containing values for each enabled <seealso cref="Node"/>.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class StatePresenter
    {
        /// <summary>
        /// Instance of <see cref="NLog.Logger">NLog.Logger</see>.
        /// </summary>
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Contains a list of delegates to be called whenever a new <seealso cref="State"/> is created.
        /// </summary>
        private readonly List<Action<State>> eventHandlersNewState;

        /// <summary>
        /// The <seealso cref="State"/> builder.
        /// </summary>
        private readonly StateBuilder stateBuilder;

        /// <summary>
        /// The scanner task used for periodically updating <seealso cref="Node"/> values and building a new <seealso cref="State"/>.
        /// </summary>
        private ITargetBlock<DateTimeOffset> scannerTask;

        /// <summary>
        /// The <seealso cref="CancellationTokenSource"/> for <seealso cref="scannerTask"/>
        /// </summary>
        private CancellationTokenSource scannerTaskCts;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatePresenter"/> class.
        /// </summary>
        public StatePresenter()
        {
            LogAllExceptions();
            this.NodeSupervisor = new NodeSupervisor();
            this.stateBuilder = new StateBuilder(this.NodeSupervisor);
            this.eventHandlersNewState = new List<Action<State>>();
        }

        /// <summary>
        /// Gets or sets the scanner interval (how often to update <seealso cref="Node"/> values, build the <seealso cref="State"/>, and fire the "new state created" event).
        /// </summary>
        public static TimeSpan NodeScannerInterval { get; set; } = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// Gets the node supervisor.
        /// </summary>
        public NodeSupervisor NodeSupervisor { get; }

        /// <summary>
        /// <para>Initializes the scanner by building <seealso cref="Node"/>s and starting the <seealso cref="scannerTask"/>.</para>
        /// <para>Call this method AFTER initializing your derived <seealso cref="Node"/> classes.</para>
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void StartScannerLoop()
        {
            this.NodeSupervisor.BuildNodeCollections();

            // create the token source
            this.scannerTaskCts = new CancellationTokenSource();

            // set the task
            this.scannerTask = this.CreateScannerInfiniteLoopTask((now, ct) => this.UpdateEnabledNodesAsync(), this.scannerTaskCts.Token);

            // start the task and post the time
            this.scannerTask.Post(DateTimeOffset.Now);
        }

        /// <summary>
        /// Cancels and nullifies the <seealso cref="scannerTask"/> and <seealso cref="scannerTaskCts"/>
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void StopScannerLoop()
        {
            // CancellationTokenSource implements IDisposable.
            using (this.scannerTaskCts)
            {
                // Cancel.  This will cancel the task.
                this.scannerTaskCts.Cancel();
            }

            // Set everything to null, since the references
            // are on the class level and keeping them around
            // is holding onto invalid state.
            this.scannerTaskCts = null;
            this.scannerTask = null;
        }

        /// <summary>
        /// Wrapper method for <see cref="NodeSupervisor.Reset"/>
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void ResetAllNodes()
        {
            this.NodeSupervisor.Reset();
        }

        /// <summary>
        /// Adds the given <paramref name="eventHandler"/> action to the list of delegates to be called whenever a new <seealso cref="State"/> is created (<see cref="eventHandlersNewState"/>).
        /// </summary>
        /// <param name="eventHandler">
        /// The event handler.
        /// </param>
        // ReSharper disable once UnusedMember.Global
        public void AddEventHandler_NewStateCreated(Action<State> eventHandler)
        {
            this.eventHandlersNewState.Add(eventHandler);
        }

        /// <summary>
        /// Disposes and resets update tasks for all initialized <seealso cref="Node"/>s.
        /// </summary>
        private static void ResetNodeUpdateTasks()
        {
            // reset all Node update tasks
            foreach (Node node in Node.InitializedNodes)
            {
                TaskStatus taskStatus = node.TaskManager.GetUpdateTaskStatus();

                switch (taskStatus)
                {
                    // dispose and reset for these statuses
                    case TaskStatus.WaitingToRun: // when null
                    case TaskStatus.Canceled:
                    case TaskStatus.RanToCompletion:
                    case TaskStatus.Faulted:
                        Logger.Debug($"{node.T()} Resetting _updateTask (task status: {taskStatus.ToString()})");
                        node.TaskManager.DisposeUpdateTask();
                        node.TaskManager.ResetUpdateTaskCts();
                        break;

                    // do nothing for these statuses
                    case TaskStatus.Created:
                        break;
                    case TaskStatus.Running:
                        break;
                    case TaskStatus.WaitingForActivation:
                        break;
                    case TaskStatus.WaitingForChildrenToComplete:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Catches all Exceptions thrown anywhere in the current app domain.
        /// </summary>
        private static void LogAllExceptions()
        {
            AppDomain.CurrentDomain.FirstChanceException += ExceptionLogHandler;
        }

        /// <summary>
        /// The exception log handler.
        /// </summary>
        /// <param name="source">
        /// The class where the exception was thrown.
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/> for the exception.
        /// </param>
        private static void ExceptionLogHandler(object source, FirstChanceExceptionEventArgs e)
        {
            Logger.Error($"\n\tError: '{e.Exception.Message}' thrown in '{source?.GetType()}: {e.Exception.StackTrace}'\n");
        }

        /// <summary>
        /// <para>Creates an "infinite loop" for <see cref="scannerTask"/> to periodically execute the given <paramref name="action"/>, process the state, and reset node update tasks.</para>
        /// <para>Execution period defined by <see cref="NodeScannerInterval"/>.</para>
        /// <para>(Idea taken from: https://stackoverflow.com/a/13712646 )</para>
        /// </summary>
        /// <param name="action">The action to periodically execute.</param>
        /// <param name="cancellationToken">The cancellation token to pass to the <paramref name="action"/>.</param>
        /// <returns>A new <seealso cref="ActionBlock{TInput}"/> for the given <paramref name="action"/>.</returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private ITargetBlock<DateTimeOffset> CreateScannerInfiniteLoopTask(Func<DateTimeOffset, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            // TODO: Look into using a BackgroundWorker instead of ActionBlock

            // Validate parameters.
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Declare the block variable, it needs to be captured.
            ActionBlock<DateTimeOffset> block = null;

            // Create the block, it will call itself, so
            // you need to separate the declaration and
            // the assignment.
            // Async so you can wait easily when the
            // delay comes.
            block = new ActionBlock<DateTimeOffset>(
                async now =>
                    {
                        // Perform the action.  Wait on the result.
                        await action(now, cancellationToken).ConfigureAwait(false);

                        // Wait.
                        await Task.Delay(NodeScannerInterval, cancellationToken).ConfigureAwait(false);

                        // Post the action back to the block.
                        // ReSharper disable once AccessToModifiedClosure
                        block?.Post(DateTimeOffset.Now);

                        this.BuildAndSignalNewState();
                        ResetNodeUpdateTasks();
                    },
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = cancellationToken
                });

            // Return the block.
            return block;
        }

        /// <summary>
        /// Builds a new <seealso cref="State"/> object and passes it to the <see cref="eventHandlersNewState"/> delegates.
        /// </summary>
        private void BuildAndSignalNewState()
        {
            // TODO: OPTIMIZE - Fix unnecessary delay due to execution time + delay time (delay time should be reduced by the execution time, but still maintain some minimum delay time so the tasks don't go haywire)
            // await Task.Delay(NodeScannerInterval);

            // TODO: UNFINISHED - Shift the "latest" value to "previous" and copy each value on the state to "latest" for each node.
            // This is to prevent situations where e.g. MapTime from having equal "current" and "previous" values despite the current and previous values on the State having different values.

            // do its best to set as many Node values as possible on the State (not all Nodes will have finished updating yet).
            // build the state
            State state = this.stateBuilder.Build();

            // pass the state to the event handlers
            this.ProcessEventHandlers_NewStateCreated(state);
        }

        /// <summary>
        /// Runs the "update value" tasks for each enabled <seealso cref="Node"/>. Completes when *any* one of the enabled node update tasks complete.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task UpdateEnabledNodesAsync()
        {
            Logger.Debug("Starting update tasks for enabled Nodes...");

            // var leafNodes = NodeSupervisor.LeafNodes;
            NodeCollection presentationEnabledNodes = this.NodeSupervisor.EnabledNodes;

            var tasks = new List<Task>();

            foreach (Node node in presentationEnabledNodes)
            {
                Task task = node.TaskManager.UpdateAsync();
                tasks.Add(task);
            }

            try
            {
                await Task.WhenAny(tasks);
            }
            catch (AggregateException ae)
            {
                Logger.Error(ae.Message);

                foreach (Exception e in ae.InnerExceptions)
                {
                    Logger.Error(e.Message);
                }
            }
            catch (TaskCanceledException tce)
            {
                Logger.Error(tce.Message);
            }
            catch (OperationCanceledException oce)
            {
                Logger.Error(oce.Message);
            }
        }

        /// <summary>
        /// <para>Executes the delegates of <seealso cref="eventHandlersNewState"/> subscribed to the "new <seealso cref="State"/> created" event.</para>
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        private void ProcessEventHandlers_NewStateCreated(State state)
        {
            this.eventHandlersNewState.ForEach(eventHandler => eventHandler(state));
        }
    }
}
