using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RTSP.Core
{
    public class StatePresenter
    {
        public NodeSupervisor NodeSupervisor { get; private set; }
        private readonly StateBuilder _stateBuilder;
        private readonly List<Action<State>> _eventHandlers_NewState;

        // TODO: Load this from config
        private TimeSpan _scannerInterval = TimeSpan.FromMilliseconds(5000);

        public StatePresenter()
        {
            NodeSupervisor = new NodeSupervisor();
            _stateBuilder = new StateBuilder(NodeSupervisor);
            _eventHandlers_NewState = new List<Action<State>>();
        }

        public async Task StartAsync()
        {
            NodeSupervisor.BuildNodeCollections();

            var cts = new CancellationTokenSource();

            while (true)
            {
                Debug.WriteLine("------------------------------------------------------");

                if (cts.IsCancellationRequested)
                    return;

                var leafNodes = NodeSupervisor.LeafNodes;

                foreach (var node in leafNodes)
                {
                    if (node.GetUpdateTaskStatus() != TaskStatus.Running)
                    {
                        Debug.WriteLine($"{node.T()} UpdateAsync() START...");
                        node.UpdateAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Debug.WriteLine($"{node.T()} -----------UpdateAsync() ALREADY RUNNING.");
                    }

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
    }
}
