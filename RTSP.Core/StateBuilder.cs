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
                
                // only include this node's property name on the state if the name is defined
                StatePropertyAttribute statePropertyAttribute = node.GetStatePropertyAttribute();

                if (statePropertyAttribute?.Name == null)
                {
                    continue;
                }

                state[statePropertyAttribute.Name] = node.GetValue();
            }

            return state;
        }
    }
}
