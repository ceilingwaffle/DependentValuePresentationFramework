using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSP.Core
{
    public class NodeCollection
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

            _nodes[node.GetType()] = node;
        }

        public List<KeyValuePair<Type, Node>> ToList()
        {
            return _nodes.ToList();
        }

        public IEnumerable<Node> ToEnumerable()
        {
            return _nodes.ToList().Select((node) => { return node.Value; });
        }

        public int Count()
        {
            return _nodes.Count;
        }

        public bool TryGetValue(Type nodeType, out Node node)
        {
            return _nodes.TryGetValue(nodeType, out node);
        }
    }
}
