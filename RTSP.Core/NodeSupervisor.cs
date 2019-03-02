using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // TODO: replace all these uses of Select with a custom Class for Dictionary<Type, Node> with a GetNodes() method
            var masterNodesList = RootNodes.ToList().Select((n) => { return n.Value; });

            LeafNodes = RecursiveChildLeafGet(masterNodesList);
        }

        private NodeCollection RecursiveChildLeafGet(IEnumerable<Node> nodes)
        {
            var leafs = new NodeCollection();

            // TODO: Replace recursion with while loop or task? Possible stack overflow exception...
            foreach (var node in nodes)
            {
                if (! node.HasChildren())
                {
                    leafs.Add(node);
                }
                else
                {
                    var children = node.Children.ToList().Select((n) => { return n.Value; });
                    var childrenLeafs = RecursiveChildLeafGet(children);

                    // merge
                    childrenLeafs.ToList().ForEach(n => leafs.Add(n.Value));
                }

            }

            return leafs;
        }


    }
}
