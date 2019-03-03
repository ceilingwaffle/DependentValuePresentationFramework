using System;
using System.Collections.Generic;

namespace RTSP.Core
{
    public class NodeSupervisor
    {
        /// <summary>
        /// Nodes not depending on values from any other Nodes.
        /// </summary>
        internal NodeCollection RootNodes { get; private set; }
        /// <summary>
        /// Nodes having no Nodes depedent on their values.
        /// </summary>
        internal NodeCollection LeafNodes { get; private set; }

        //private static int _depth = -1;

        public NodeSupervisor()
        {
            RootNodes = new NodeCollection();
            LeafNodes = new NodeCollection();
        }

        public void AddRootNodes(params Node[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node.HasParents())
                    throw new Exception($"Node {node.GetType().ToString()} has at least one parent and is therefore not a root Node.");

                RootNodes.Add(node);
            }
        }

        internal void BuildLeafNodes()
        {
            var rootNodesList = RootNodes.ToEnumerable();

            LeafNodes = CollectLeafNodes(rootNodesList);
        }

        private NodeCollection CollectLeafNodes(IEnumerable<Node> nodes)
        {
            var leaves = new NodeCollection();

            var unvisited = new Stack<Node>();

            foreach (var node in nodes)
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
                        unvisited.Push(child);
                    }
                }
            }

            return leaves;
        }


    }
}
