using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DVPF.Core
{
    // TODO: REFACTOR - Make code styling choices consistent across classes (e.g. underscore prefixes) - use a C# linter or something...

    // TODO: REFACTOR - Comment all publicly accessible methods and fields.

    // TODO: REFACTOR - Use Stylecop to enforce a maximum number of characters per line

    public class StatePresenter
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public NodeSupervisor NodeSupervisor { get; private set; }
        public static TimeSpan ScannerInterval { get; set; } = TimeSpan.FromMilliseconds(200);

        private readonly StateBuilder _stateBuilder;
        private readonly List<Action<State>> _eventHandlers_NewState;

        private ITargetBlock<DateTimeOffset> _scannerTask;
        private CancellationTokenSource _scannerTaskCTS;

        public StatePresenter()
        {
            LogAllExceptions();
            NodeSupervisor = new NodeSupervisor();
            _stateBuilder = new StateBuilder(NodeSupervisor);
            _eventHandlers_NewState = new List<Action<State>>();
        }

        public void StartScannerLoop()
        {
            NodeSupervisor.BuildNodeCollections();

            // Create the token source.
            _scannerTaskCTS = new CancellationTokenSource();

            // Set the task.
            _scannerTask = CreateScannerInfiniteLoopTask((now, ct) => DoWorkAsync(ct), _scannerTaskCTS.Token);

            // Start the task.  Post the time.
            _scannerTask.Post(DateTimeOffset.Now);
        }

        public void StopScannerLoop()
        {
            // CancellationTokenSource implements IDisposable.
            using (_scannerTaskCTS)
            {
                // Cancel.  This will cancel the task.
                _scannerTaskCTS.Cancel();
            }

            // Set everything to null, since the references
            // are on the class level and keeping them around
            // is holding onto invalid state.
            _scannerTaskCTS = null;
            _scannerTask = null;
        }

        /// <summary>
        /// https://stackoverflow.com/a/13712646
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private ITargetBlock<DateTimeOffset> CreateScannerInfiniteLoopTask(Func<DateTimeOffset, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            // TODO: Look into using a BackgroundWorker instead of ActionBlock

            // Validate parameters.
            if (action == null) throw new ArgumentNullException("action");

            // Declare the block variable, it needs to be captured.
            ActionBlock<DateTimeOffset> block = null;

            // Create the block, it will call itself, so
            // you need to separate the declaration and
            // the assignment.
            // Async so you can wait easily when the
            // delay comes.
            block = new ActionBlock<DateTimeOffset>(async now =>
            {
                // Perform the action.  Wait on the result.
                await action(now, cancellationToken).
                    // Doing this here because synchronization context more than
                    // likely *doesn't* need to be captured for the continuation
                    // here.  As a matter of fact, that would be downright
                    // dangerous.
                    ConfigureAwait(false);

                // Wait.
                await Task.Delay(ScannerInterval, cancellationToken).
                    // Same as above.
                    ConfigureAwait(false);

                // Post the action back to the block.
                block.Post(DateTimeOffset.Now);

                _ProcessState();
                _ResetNodeUpdateTasks();

            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken
            });

            // Return the block.
            return block;
        }

        private static void _ResetNodeUpdateTasks()
        {
            // reset all Node update tasks
            foreach (var node in Node.InitializedNodes)
            {
                TaskStatus taskStatus = node.TaskManager.GetUpdateTaskStatus();

                if (taskStatus == TaskStatus.WaitingToRun // when null
                    || taskStatus == TaskStatus.Canceled
                    || taskStatus == TaskStatus.RanToCompletion
                    || taskStatus == TaskStatus.Faulted
                )
                {
                    _logger.Debug($"{node.T()} Resetting _updateTask (task status: {taskStatus.ToString()})");

                    node.TaskManager.DisposeUpdateTask();
                    node.TaskManager.ResetUpdateTaskCts();
                }
            }
        }

        private void _ProcessState()
        {
            // TODO: OPTIMIZE - Fix unnecessary delay due to execution time + delay time (delay time should be reduced by the execution time, but still maintain some minimum delay time so the tasks don't go haywire)
            //await Task.Delay(ScannerInterval);

            // TODO: UNFINISHED - Shift the "latest" value to "previous" and copy each value on the state to "latest" for each node.
            //       This is to prevent situations where e.g. MapTime from having equal "current" and "previous" values despite the current and previous values on the State having different values.

            // do its best to set as many Node values as possible on the State (not all Nodes will have finished updating yet).
            // build the state
            State state = _stateBuilder.Build();

            // pass the state to the event handlers
            ProcessEventHandlers_NewStateCreated(state);
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            _logger.Debug("------------------------------------------------------");

            //var leafNodes = NodeSupervisor.LeafNodes;
            var presentationEnabledNodes = NodeSupervisor.EnabledNodes;

            var tasks = new List<Task>();

            foreach (var node in presentationEnabledNodes)
            {
                //if (node.TaskManager.GetUpdateTaskStatus() != TaskStatus.Running)
                //{
                //    node.TaskManager.DisposeUpdateTask();
                //    node.TaskManager.ResetUpdateTaskCTS();
                //}

                var task = node.TaskManager.UpdateAsync();

                tasks.Add(task);
            }

            try
            {
                await Task.WhenAny(tasks);
            }
            catch (AggregateException ae)
            {
                _logger.Error(ae.Message);

                foreach (var e in ae.InnerExceptions)
                {
                    _logger.Error(e.Message);
                }
            }
            catch (TaskCanceledException tce)
            {
                _logger.Error(tce.Message);
            }
            catch (OperationCanceledException oce)
            {
                _logger.Error(oce.Message);
            }


        }

        public void ResetAllNodes()
        {
            NodeSupervisor.Reset();
        }

        public void AddEventHandler_NewStateCreated(Action<State> eventHandler)
        {
            _eventHandlers_NewState.Add(eventHandler);
        }

        private void ProcessEventHandlers_NewStateCreated(State state)
        {
            _eventHandlers_NewState.ForEach(eventHandler => eventHandler(state));
        }

        private void LogAllExceptions()
        {
            AppDomain.CurrentDomain.FirstChanceException += ExceptionLogHandler;
        }

        private void ExceptionLogHandler(object source, FirstChanceExceptionEventArgs e)
        {
            _logger.Error($"\n\tError: '{e.Exception.Message}' thrown in '{source.GetType()}: {e.Exception.StackTrace}'\n");
        }
    }
}
