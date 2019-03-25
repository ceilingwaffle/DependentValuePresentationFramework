using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace RTSP.Core
{
    public class StatePresenter
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public NodeSupervisor NodeSupervisor { get; private set; }
        private readonly StateBuilder _stateBuilder;
        private readonly List<Action<State>> _eventHandlers_NewState;

        // TODO: Load this from config
        private TimeSpan _scannerInterval = TimeSpan.FromMilliseconds(100);

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

                foreach (var node in presentationEnabledNodes)
                {
                    //if (node.TaskManager.GetUpdateTaskStatus() != TaskStatus.Running)
                    //{
                    //    node.TaskManager.DisposeUpdateTask();
                    //    node.TaskManager.ResetUpdateTaskCTS();
                    //}

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    node.TaskManager.UpdateAsync().ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                }

                await Task.Delay(_scannerInterval);

                // do its best to set as many Node values as possible on the State (not all Nodes will have finished updating yet).
                // build the state
                State state = _stateBuilder.Build();

                // pass the state to the event handlers
                ProcessEventHandlers_NewStateCreated(state);
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
