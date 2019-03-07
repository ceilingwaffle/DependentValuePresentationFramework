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

            // build a State containing every node value
            foreach (var node in nodes)
            {
                if (! node.IsStatePresentable())
                {
                    continue;
                }

                var value = node.GetValue();



                // TODO: Assign value to the corresponding state property
            }

            // TODO: either:
            // a) Dynamic object with properties (node type names) defined at runtime
            // b) Emit/Reflection to add properties to the State class
            //State.MapId; 

            return new State();
        }
    }
}
