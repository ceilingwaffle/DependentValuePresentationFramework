using System;
using System.Collections.Generic;

namespace RTSP.Core
{
    public class NodeSupervisor
    {
        /// <summary>
        /// Nodes not depending on values from any other Nodes.
        /// </summary>
        //internal NodeCollection RootNodes { get; private set; } = new NodeCollection();
        /// <summary>
        /// Nodes having no Nodes depedent on their values.
        /// </summary>
        //internal NodeCollection LeafNodes { get; private set; } = new NodeCollection();

        internal NodeCollection EnabledNodes { get; private set; } = new NodeCollection();

        public NodeSupervisor()
        {
        }

        internal void BuildNodeCollections()
        {
            EnabledNodes = _CollectEnabledNodes(Node.InitializedNodes);
            //RootNodes = _CollectRootNodes(Node.InitializedNodes);
            //LeafNodes = _CollectLeafNodes(RootNodes);
        }

        private NodeCollection _CollectEnabledNodes(NodeCollection initializedNodes)
        {
            var enabled = new NodeCollection();

            foreach (var node in initializedNodes)
            {
                if (node.IsEnabled())
                {
                    enabled.Add(node);
                }

            }

            return enabled;
        }

        //private NodeCollection _CollectRootNodes(NodeCollection initializedNodesCollection)
        //{
        //    var roots = new NodeCollection();

        //    foreach (var node in initializedNodesCollection)
        //    {
        //        if (!node.HasParents())
        //        {
        //            roots.Add(node);
        //        }
        //    }

        //    return roots;
        //}

        //private NodeCollection _CollectLeafNodes(NodeCollection rootNodesCollection)
        //{
        //    var leaves = new NodeCollection();

        //    var unvisited = new Stack<Node>();

        //    foreach (var node in rootNodesCollection)
        //    {
        //        unvisited.Push(node);
        //    }

        //    while (unvisited.Count > 0)
        //    {
        //        var node = unvisited.Pop();

        //        if (!node.HasChildren())
        //        {
        //            leaves.Add(node);
        //        }
        //        else
        //        {
        //            foreach (var child in node.Children)
        //            {
        //                if (!unvisited.Contains(child))
        //                {
        //                    unvisited.Push(child);
        //                }
        //            }
        //        }
        //    }

        //    return leaves;
        //}

        internal void Reset()
        {
            //RootNodes = new NodeCollection();
            //LeafNodes = new NodeCollection();
            Node.ResetInitializedNodes();
            Node.ResetNodeStatePropertyNames();
            EnabledNodes = GetEnabledNodes();
        }

        internal NodeCollection GetEnabledNodes()
        {
            return EnabledNodes;
        }

    }
}
