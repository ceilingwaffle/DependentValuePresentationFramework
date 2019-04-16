namespace DVPF.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Supervises and manages <see cref="Node"/>s in the app.
    /// </summary>
    public class NodeSupervisor
    {
        ///// <summary>
        ///// Nodes not depending on values from any other Nodes.
        ///// </summary>
        ////internal NodeCollection RootNodes { get; private set; } = new NodeCollection();

        ///// <summary>
        ///// Nodes having no Nodes dependent on their values.
        ///// </summary>
        ////internal NodeCollection LeafNodes { get; private set; } = new NodeCollection();

        /// <summary>
        /// Gets all <see cref="StatePropertyAttribute.Enabled"/> nodes (node values being presented as properties on the <see cref="State"/> output).
        /// </summary>
        internal NodeCollection EnabledNodes { get; private set; } = new NodeCollection();

        /// <summary>
        /// Initializes a new empty <see cref="NodeCollection"/> and assigns it to <see cref="Node.InitializedNodes"/>
        /// </summary>
        public static void ResetInitializedNodes()
        {
            Node.InitializedNodes = new NodeCollection();
        }

        /// <summary>
        /// <para>Gets the <see cref="Node"/> associated with the specified node <see cref="Type"/> stored in <see cref="Node.InitializedNodes"/></para>
        /// <para>See: <seealso cref="NodeCollection.TryGetValue"/></para>
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
        public bool TryGetInitializedNode(Type nodeType, out Node node)
        {
            return Node.InitializedNodes.TryGetValue(nodeType, out node);
        }

        /// <summary>
        /// Processes the <see cref="Node.InitializedNodes"/> and assigns them to various collections.
        /// </summary>
        internal void BuildNodeCollections()
        {
            this.EnabledNodes = this.FilterEnabledNodes(Node.InitializedNodes);
            ////RootNodes = FilterRootNodes(Node.InitializedNodes);
            ////LeafNodes = FilterLeafNodes(RootNodes);
        }

        /// <summary>
        /// Returns a filtered-subset of <see cref="Node"/>s from the given <paramref name="nodes"/> where <see cref="StatePropertyAttribute.Enabled"/> is set to true for those nodes.
        /// </summary>
        /// <param name="nodes">
        /// The nodes to be filtered.
        /// </param>
        /// <returns>
        /// A <see cref="NodeCollection"/> containing the enabled nodes.
        /// </returns>
        internal NodeCollection FilterEnabledNodes(NodeCollection nodes)
        {
            var filtered = new NodeCollection();

            foreach (var node in nodes)
            {
                if (node.IsEnabled())
                {
                    filtered.Add(node);
                }
            }

            return filtered;
        }

        /// <summary>
        /// Returns a filtered-subset of <see cref="Node"/>s from the given <paramref name="nodes"/> containing only "root" nodes (nodes without preceder-nodes).
        /// </summary>
        /// <param name="nodes">
        /// The nodes to be filtered.
        /// </param>
        /// <returns>
        /// A <see cref="NodeCollection"/> containing the root nodes.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        internal NodeCollection FilterRootNodes(NodeCollection nodes)
        {
            var roots = new NodeCollection();

            foreach (var node in nodes)
            {
                if (!node.HasPreceders())
                {
                    roots.Add(node);
                }
            }

            return roots;
        }

        /// <summary>
        /// Returns a filtered-subset of <see cref="Node"/>s from the given <paramref name="nodes"/> containing only "leaf" nodes (nodes without follower-nodes).
        /// </summary>
        /// <param name="nodes">
        /// The nodes to be filtered.
        /// </param>
        /// <returns>
        /// A <see cref="NodeCollection"/> containing the enabled nodes.
        /// </returns>
        internal NodeCollection FilterLeafNodes(NodeCollection nodes)
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

                if (!node.HasFollowers())
                {
                    leaves.Add(node);
                }
                else
                {
                    foreach (var follower in node.Followers)
                    {
                        if (!unvisited.Contains(follower))
                        {
                            unvisited.Push(follower);
                        }
                    }
                }
            }

            return leaves;
        }

        /// <summary>
        /// Resets <see cref="Node.InitializedNodes"/>, <see cref="Node.nodeStatePropertyNames"/>, and re-builds <see cref="EnabledNodes"/>.
        /// </summary>
        internal void Reset()
        {
            ////RootNodes = new NodeCollection();
            ////LeafNodes = new NodeCollection();
            ResetInitializedNodes();
            Node.ResetNodeStatePropertyNames();
            this.EnabledNodes = this.FilterEnabledNodes(Node.InitializedNodes);
        }
    }
}
