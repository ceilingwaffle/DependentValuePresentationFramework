using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DVPF.Core
{
    public class NodeCollection : IEnumerable<Node>
    {
        private Dictionary<Type, Node> _nodes;

        public NodeCollection(params Node[] nodes)
        {
            _nodes = new Dictionary<Type, Node>();

            foreach (var node in nodes)
                Add(node);
        }

        public void Add(Node node)
        {
            if (node is null)
                throw new ArgumentNullException();

            //_nodes[node.GetType()] = node;
            _nodes.Add(node.GetType(), node);
        }

        public List<KeyValuePair<Type, Node>> ToList()
        {
            return _nodes.ToList();
        }

        public int Count()
        {
            return _nodes.Count;
        }

        public bool TryGetValue(Type nodeType, out Node node)
        {
            return _nodes.TryGetValue(nodeType, out node);
        }

        public bool Exists(Node targetNode)
        {
            bool nodeExists = TryGetValue(targetNode.GetType(), out var existingNode);

            return nodeExists;
        }

        #region Implementation of IEnumerable
        public IEnumerator<Node> GetEnumerator()
        {
            return _nodes.ToList().Select((node) => { return node.Value; }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}
