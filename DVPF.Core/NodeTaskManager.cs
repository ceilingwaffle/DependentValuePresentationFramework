namespace DVPF.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using NLog;

    /// <inheritdoc />
    /// <summary>
    /// Manages <see cref="Task"/>s for a single <see cref="Node"/>
    /// </summary>
    internal class NodeTaskManager : IDisposable
    {
        /// <summary>
        /// Instance of <see cref="NLog.Logger">NLog.Logger</see>.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The node being managed.
        /// </summary>
        private readonly Node node;

        /// <summary>
        /// <para>A key-value collection of <see cref="Node"/> types (key) and their <see cref="CancellationTokenSource"/> (value) for nodes that follow this <see cref="node"/>.</para>
        /// <para>Used for cancelling <see cref="updateTask"/> on followers of the <see cref="node"/></para>
        /// </summary>
        private readonly Dictionary<Type, CancellationTokenSource> followerTaskCtsList = new Dictionary<Type, CancellationTokenSource>();

        /// <summary>
        /// Lock for <see cref="followerTaskCtsList"/>
        /// </summary>
        private readonly object followerTaskCtsListLock = new object();

        // TODO: UNFINISHED - Load updateTimeLimit from config

        /// <summary>
        /// The maximum time the <see cref="updateTask"/> can spend executing before signaling <see cref="updateTaskCts"/> to cancel.
        /// </summary>
        private readonly TimeSpan updateTimeLimit = TimeSpan.FromMilliseconds(10000);

        /// <summary>
        /// Task to get and set the value of <see cref="node"/>.
        /// </summary>
        private Task updateTask;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> for <see cref="updateTask"/>.
        /// </summary>
        private CancellationTokenSource updateTaskCts;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeTaskManager"/> class.
        /// </summary>
        /// <param name="node">
        /// The node to be managed by this manager.
        /// </param>
        public NodeTaskManager(Node node)
        {
            this.node = node;
        }

        /// <inheritdoc />
        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            this.updateTaskCts.Dispose();
        }

        /// <summary>
        /// Creates a new <see cref="updateTaskCts"/>, and assigns it to this manager and <see cref="followerTaskCtsList"/> for all nodes preceding this node.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        internal void ResetUpdateTaskCts()
        {
            this.updateTaskCts = new CancellationTokenSource();

            foreach (var preceder in this.node.Preceders)
            {
                preceder.TaskManager.AddFollowerCts(this.node.GetType(), this.updateTaskCts);
            }

            Logger.Debug($"{this.node.T()} _ResetUpdateTaskCTS().");
        }

        /// <summary>
        /// Sets <see cref="updateTask"/> to null.
        /// </summary>
        internal void DisposeUpdateTask()
        {
            if (this.updateTask == null)
            {
                return;
            }

            this.updateTask = null;

            Logger.Debug($"{this.node.T()} DisposeUpdateTask().");
        }

        /// <summary>
        /// Conditionally decides whether or not to create a new <see cref="updateTask"/> or await the existing one.
        /// </summary>
        /// <returns>
        /// The update task <see cref="updateTask"/>.
        /// </returns>
        internal async Task UpdateAsync()
        {
            try
            {
                Logger.Debug($"{this.node.T()} UpdateAsync() START...");

                if (this.updateTask?.Status == TaskStatus.Running)
                {
                    Logger.Debug("{0} _updateTask already running (_updateTask?.Status == TaskStatus.Running).", this.node.T());
                }

                if (this.updateTask is null)
                {
                    this.updateTask = this.ManageValueUpdate();
                }

                if (!this.updateTask.IsCanceled)
                {
                    try
                    {
                        await this.updateTask;
                    }
                    catch (AggregateException ae)
                    {
                        Logger.Error(ae.Message);

                        foreach (var e in ae.InnerExceptions)
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
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// Gets the <see cref="TaskStatus"/> of the <see cref="updateTask"/>
        /// </summary>
        /// <returns>
        /// The <see cref="TaskStatus"/> of the <see cref="updateTask"/>
        /// </returns>
        internal TaskStatus GetUpdateTaskStatus()
        {
            // TODO: REFACTOR - Something else like a custom UpdateTaskStatus class which extends TaskStatus but has a custom null status.
            return this.updateTask?.Status ?? TaskStatus.WaitingToRun;
        }

        /// <summary>
        /// Adds the given <paramref name="cts"/> to the node of type <paramref name="followerNodeType"/> (must be a follower of this <see cref="node"/>).
        /// </summary>
        /// <param name="followerNodeType">
        /// The target node to be assigned the given <paramref name="cts"/>.
        /// </param>
        /// <param name="cts">
        /// The <see cref="CancellationTokenSource"/> to be added to <see cref="updateTaskCts"/> on the follower node of type <paramref name="followerNodeType"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="followerNodeType"/> is not a follower of <see cref="node"/>.
        /// </exception>
        private void AddFollowerCts(Type followerNodeType, CancellationTokenSource cts)
        {
            // TODO - Write unit test to assert that this exception gets thrown
            if (!this.node.Followers.TryGetValue(followerNodeType, out _))
            {
                throw new ArgumentException($"{followerNodeType} is not a follower of {this.node.GetType()}.", nameof(followerNodeType));
            }

            lock (this.followerTaskCtsListLock)
            {
                this.followerTaskCtsList[followerNodeType] = cts;
            }
        }

        /// <summary>
        /// Creates a new update task if one is not already created. If an update task already exists (and is not cancelled), await its finish.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ManageValueUpdate()
        {
            try
            {
                Logger.Debug($"{this.node.T()} Running Update Task...");

                // TODO: UNFINISHED - _updateTaskCts.CancelAfter(t)  <- read "t" from Class Attribute, configurable per node
                this.updateTaskCts.CancelAfter(this.updateTimeLimit);

                if (this.NullifyValueIfUpdateTaskIsCancelled())
                {
                    return;
                }

                await Task.WhenAll(this.GetPrecederUpdateTasks());

                if (this.NullifyValueIfUpdateTaskIsCancelled())
                {
                    return;
                }

                var value = await this.node.DetermineValueAsync();

                if (this.NullifyValueIfUpdateTaskIsCancelled())
                {
                    return;
                }

                this.node.SetValue(value);

                if (this.node.ValueChanged())
                {
                    this.node.HandleValueChanged(value);
                }

                this.CancelFollowerTasksIfValueUpdated();

                Logger.Debug($"{this.node.T()} Update Task completed: {this.updateTask?.Status.ToString()}");
            }
            catch (AggregateException ae)
            {
                Logger.Error($"Caught Task Error: {ae.Message}");
            }
        }

        /// <summary>
        /// Sets the current value of the <see cref="node"/> to null if its <see cref="updateTask"/> is cancelled.
        /// </summary>
        /// <returns>
        /// true if <see cref="updateTask"/> is cancelled (and <see cref="node"/> value nullified), false if not.
        /// </returns>
        private bool NullifyValueIfUpdateTaskIsCancelled()
        {
            if (!this.updateTaskCts.IsCancellationRequested)
            {
                return false;
            }

            Logger.Debug($"{this.node.T()} Cancellation was requested. Resetting value to null. (a)");

            this.node.NullifyValueWithoutShiftingToPrevious();

            return true;
        }

        /// <summary>
        /// This Node's value changed, meaning all follower node values are now potentially expired, so issue a cancel to all follower update tasks.
        /// </summary>
        private void CancelFollowerTasksIfValueUpdated()
        {
            if (!this.node.ValueChanged())
            {
                Logger.Debug($"{this.node.T()} Value was same: ({this.node.GetPreviousValue()} -> {this.node.GetValue()}).");
            }
            else
            {
                Logger.Debug($"{this.node.T()} Value changed: ({this.node.GetPreviousValue()} -> {this.node.GetValue()}).");

                var toBeVisited = new HashSet<Node>();

                foreach (Node follower in this.node.Followers)
                {
                    toBeVisited.Add(follower);
                }

                while (toBeVisited.Count > 0)
                {
                    Node targetDescendent = toBeVisited.First();
                    toBeVisited.Remove(targetDescendent);

                    Logger.Debug($"{this.node.T()} Issuing cancel to follower {targetDescendent.T()}...");
                    targetDescendent.TaskManager.CancelFollowerTasks();

                    // Check the StateAttribute of Node allowing toggle of "nullify value if any parent value changes", then conditionally check for it and run NullifyValueWithoutShiftingToPrevious() if true.
                    if (targetDescendent.IsStrictValue())
                    {
                        Logger.Debug($"{targetDescendent.T()} Strict value TRUE. Nullifying value...");
                        targetDescendent.NullifyValueWithoutShiftingToPrevious();
                    }

                    foreach (var follower in targetDescendent.Followers)
                    {
                        toBeVisited.Add(follower);
                    }
                }
            }
        }

        /// <summary>
        /// Issues a cancel on the <see cref="CancellationTokenSource"/> of all follower-node update tasks.
        /// </summary>
        private void CancelFollowerTasks()
        {
            // TODO: OPTIMIZE - Use lock instead of ToList() -- ToList is used to avoid error thrown in rare chance where a thread modifies the contents while another thread is enumerating
            foreach (var ctsKv in this.node.TaskManager.followerTaskCtsList.ToList())
            {
                CancellationTokenSource cts = ctsKv.Value;

                if (cts is null)
                {
                    return;
                }

                cts.Cancel();
            }
        }

        /// <summary>
        /// Returns a list of the <see cref="updateTask"/>s for all immediate preceders of the <see cref="node"/> being managed.
        /// </summary>
        /// <returns>
        /// Array of update tasks of nodes immediately preceding this node.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private Task[] GetPrecederUpdateTasks()
        {
            var preceders = this.node.Preceders.ToArray();
            var precederTasks = new Task[this.node.Preceders.Count()];

            for (var i = 0; i < this.node.Preceders.Count(); i++)
            {
                Node preceder = preceders[i];

                if (preceder is null)
                {
                    continue;
                }

                Logger.Debug($"{this.node.T()} Requesting update from preceder: {preceder.GetType()}");
                precederTasks[i] = preceder.TaskManager.UpdateAsync();
            }

            return precederTasks;
        }
    }
}
