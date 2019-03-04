using System;
using System.Collections.Generic;

namespace RTSP.Core
{
    public class NodeSupervisor
    {
        /// <summary>
        /// Nodes not depending on values from any other Nodes.
        /// </summary>
        internal NodeCollection RootNodes { get; private set; } = new NodeCollection();
        /// <summary>
        /// Nodes having no Nodes depedent on their values.
        /// </summary>
        internal NodeCollection LeafNodes { get; private set; } = new NodeCollection();

        public NodeSupervisor()
        {
        }

        internal void BuildNodeCollections()
        {
            var initializedNodes = Node.InitializedNodes;
            this.RootNodes = _CollectRootNodes(initializedNodes);
            this.LeafNodes = _CollectLeafNodes(RootNodes);
        }

        private NodeCollection _CollectRootNodes(NodeCollection initializedNodesCollection)
        {
            var roots = new NodeCollection();

            var initializedNodes = initializedNodesCollection.ToEnumerable();

            foreach (var node in initializedNodes)
            {
                if (!node.HasParents())
                {
                    roots.Add(node);
                }
            }

            return roots;
        }

        private NodeCollection _CollectLeafNodes(NodeCollection rootNodesCollection)
        {
            var leaves = new NodeCollection();

            var unvisited = new Stack<Node>();

            foreach (var node in rootNodesCollection.ToEnumerable())
            {
                unvisited.Push(node);
            }

            while (unvisited.Count > 0)
            {
                var node = unvisited.Pop();

                if (!node.HasChildren())
                {
                    leaves.Add(node);
                }
                else
                {
                    var children = node.Children.ToEnumerable();

                    foreach (var child in children)
                    {
                        if (! unvisited.Contains(child))
                        {
                            unvisited.Push(child);
                        }
                    }
                }
            }

            return leaves;
        }

    }
}
