namespace DVPF.Core
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <inheritdoc />
    /// <summary>
    /// A collection of <see cref="T:DVPF.Core.Node" />s.
    /// </summary>
    public class NodeCollection : IEnumerable<Node>
    {
        /// <summary>
        /// Key-value store of node <see cref="Type"/> (key) and the node instance (value).
        /// </summary>
        private readonly Dictionary<Type, Node> nodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeCollection"/> class. 
        /// </summary>
        /// <param name="nodes">
        /// The <see cref="Node"/>(s) to initially contain.
        /// </param>
        public NodeCollection(params Node[] nodes)
        {
            this.nodes = new Dictionary<Type, Node>();

            foreach (var node in nodes)
            {
                this.Add(node);
            }
        }

        /// <summary>
        /// Adds a <see cref="Node"/> to the collection. If the given node type already exists in the collection, it will be overridden.
        /// </summary>
        /// <param name="node">
        /// The <see cref="Node"/> to be added.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="node"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="node"/> already exists in the collection.
        /// </exception>
        public void Add(Node node)
        {
            if (node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            try
            {
                this.nodes.Add(node.GetType(), node);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Node type {node.GetType()} already exists in this collection. Duplicates are not allowed. Original Exception message: {e.Message}");
            }
        }

        /// <summary>
        /// Converts this <see cref="NodeCollection"/> to a <see cref="List{T}"/>
        /// </summary>
        /// <returns>
        /// This <see cref="NodeCollection"/> as a <see cref="List{T}"/>.
        /// </returns>
        public List<KeyValuePair<Type, Node>> ToList()
        {
            return this.nodes.ToList();
        }

        /// <summary>
        /// Returns the number of nodes in this <see cref="NodeCollection"/>
        /// </summary>
        /// <returns>
        /// The number of nodes in this <see cref="NodeCollection"/>
        /// </returns>
        public int Count()
        {
            return this.nodes.Count;
        }

        /// <summary>
        /// Gets the <see cref="Node"/> associated with the specified node <see cref="Type"/>
        /// </summary>
        /// <param name="nodeType">
        /// The node type.
        /// </param>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <returns>
        /// true if the node exists in the collection, false if not.
        /// </returns>
        public bool TryGetValue(Type nodeType, out Node node)
        {
            return this.nodes.TryGetValue(nodeType, out node);
        }

        /// <summary>
        /// Returns true if <paramref name="targetNode"/> exists in this collection.
        /// </summary>
        /// <param name="targetNode">
        /// The <see cref="Node"/> to search for.
        /// </param>
        /// <returns>
        /// true if <paramref name="targetNode"/> exists in this collection.
        /// </returns>
        public bool Exists(Node targetNode)
        {
            var nodeExists = this.TryGetValue(targetNode.GetType(), out _);

            return nodeExists;
        }

        #region Implementation of IEnumerable
        /// <inheritdoc />
        /// <returns>
        /// The <see cref="T:System.Collections.IEnumerator" />.
        /// </returns>
        public IEnumerator<Node> GetEnumerator()
        {
            return this.nodes.ToList().Select((node) => node.Value).GetEnumerator();
        }

        /// <inheritdoc />
        /// <returns>
        /// The <see cref="T:System.Collections.IEnumerator" />.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
