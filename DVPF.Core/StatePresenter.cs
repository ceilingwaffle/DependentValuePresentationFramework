using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace DVPF.Core
{
    public class StatePresenter
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public NodeSupervisor NodeSupervisor { get; private set; }
        public static TimeSpan ScannerInterval { get; set; } = TimeSpan.FromMilliseconds(250);

        private readonly StateBuilder _stateBuilder;
        private readonly List<Action<State>> _eventHandlers_NewState;

        public StatePresenter()
        {
            LogAllExceptions();
            NodeSupervisor = new NodeSupervisor();
            _stateBuilder = new StateBuilder(NodeSupervisor);
            _eventHandlers_NewState = new List<Action<State>>();
        }

        public async Task StartAsync()
        {
            NodeSupervisor.BuildNodeCollections();

            while (true)
            {
                _logger.Debug("------------------------------------------------------");

                //var leafNodes = NodeSupervisor.LeafNodes;
                var presentationEnabledNodes = NodeSupervisor.GetEnabledNodes();

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

                await Task.Delay(ScannerInterval);

                // TODO: Shift the "latest" value to "previous" and copy each value on the state to "latest" for each node.
                //       This is to prevent situations where e.g. MapTime from having equal "current" and "previous" values despite the current and previous values on the State having different values.

                // do its best to set as many Node values as possible on the State (not all Nodes will have finished updating yet).
                // build the state
                State state = _stateBuilder.Build();

                // pass the state to the event handlers
                ProcessEventHandlers_NewStateCreated(state);

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
                        node.TaskManager.ResetUpdateTaskCTS();
                    }
                }

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
