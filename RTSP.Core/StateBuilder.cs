using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSP.Core
{
    internal class StateBuilder
    {
        private readonly NodeSupervisor _nodeSupervisor;

        public StateBuilder(NodeSupervisor nodeSupervisor)
        {
            _nodeSupervisor = nodeSupervisor;
        }

        internal State Build()
        {
            NodeCollection nodes = _nodeSupervisor.GetEnabledNodes();

            var state = new State();

            // build a State containing every node value
            foreach (var node in nodes)
            {
                if (! node.IsEnabled())
                {
                    continue;
                }

                state[node.StatePropertyName] = node.GetValue();
            }

            return state;
        }
    }
}
