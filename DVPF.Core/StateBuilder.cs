namespace DVPF.Core
{
    /// <summary>
    /// Builder of the <seealso cref="State"/>.
    /// </summary>
    internal class StateBuilder
    {
        /// <summary>
        /// Used for accessing nodes whose values will be used as <seealso cref="State"/> properties.
        /// </summary>
        private readonly NodeSupervisor nodeSupervisor;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateBuilder"/> class.
        /// </summary>
        /// <param name="nodeSupervisor">
        /// The instance of <seealso cref="NodeSupervisor"/> for <seealso cref="nodeSupervisor"/>.
        /// </param>
        public StateBuilder(NodeSupervisor nodeSupervisor)
        {
            this.nodeSupervisor = nodeSupervisor;
        }

        /// <summary>
        /// Builds the <seealso cref="State"/> containing properties from each enabled <seealso cref="Node"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="State"/>.
        /// </returns>
        internal State Build()
        {
            NodeCollection nodes = this.nodeSupervisor.EnabledNodes;

            var state = new State();

            // build a State containing every node value
            foreach (Node node in nodes)
            {
                if (!node.IsEnabled())
                {
                    continue;
                }

                // only include this node's property name on the state if the name is defined
                StatePropertyAttribute statePropertyAttribute = node.GetStatePropertyAttribute();

                if (statePropertyAttribute?.Name is null)
                {
                    continue;
                }

                // TODO: Can we cast this as a specific type, with the type defined in StatePropertyAttribute on each Node?
                state[statePropertyAttribute.Name] = node.GetValue();
            }

            return state;
        }
    }
}
