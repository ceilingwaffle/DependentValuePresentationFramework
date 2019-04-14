namespace DVPF.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    using NLog;

    /// <inheritdoc />
    /// <summary>
    /// Extend this class to define your own representation of a value.
    /// </summary>
    public abstract class Node : IDisposable
    {
        /// <summary>
        /// Instance of <see cref="DVPF.Core.NodeTaskManager"/> for this node.
        /// </summary>
        internal readonly NodeTaskManager TaskManager;

        /// <summary>
        /// Instance of <see cref="NLog.Logger">NLog.Logger</see>.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A single instance of a list containing all initialized node <see cref="StatePropertyAttribute"/> names.
        /// </summary>
        private static HashSet<string> nodeStatePropertyNames = new HashSet<string>();

        /// <summary>
        /// The value lock.
        /// </summary>
        private readonly object valueLock = new object();

        /// <summary>
        /// The capacity of the <see cref="valueLedger"/>.
        /// </summary>
        private readonly int valueLedgerCapacity = 2;

        /// <summary>
        /// A list of the current and previous values for this node.
        /// </summary>
        private LinkedList<object> valueLedger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        protected Node()
        {
            // TODO: Remove underscore prefix for methods
            this.TaskManager = new NodeTaskManager(this);
            this.TaskManager.ResetUpdateTaskCTS();
            this.InitValueLedger();
            this.AddInitializedNode(this);
        }

        /// <summary>
        /// The delegates to be fired when this node's value changes.
        /// </summary>
        public event EventHandler<NodeEventArgs> OnValueChange = delegate { };

        /// <summary>
        /// Gets the Nodes that "follow" this Node (values that depend on this node's value).
        /// This Node's value must be obtained first, before the Followers can determine their values).
        /// </summary>
        public NodeCollection Followers { get; } = new NodeCollection();

        /// <summary>
        /// Gets the Nodes that "precede" this Node (values that this Node's value depend on).
        /// The values of the preceder nodes are obtained first, before this Node's value can be determined.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public NodeCollection Preceders { get; } = new NodeCollection();

        /// <summary>
        /// Gets all <see cref="Node"/>s initialized within this application.
        /// </summary>
        internal static NodeCollection InitializedNodes { get; private set; } = new NodeCollection();

        /// <summary>
        /// <para>The method to be implemented to return a Task representing the calculated value of this node.</para>
        /// <para>If this node's value requires the use of a "preceder" node's value, those values can be accessed
        /// using the method <see cref="NodeCollection.TryGetValue"/> of <see cref="Preceders"/> providing
        /// the preceder node's type as the <see cref="Type"/> argument.</para>
        /// <para>The overridden method should be implemented using the async modifier.</para>
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> representing the calculated-value of this node.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public abstract Task<object> DetermineValueAsync();

        /// <summary>
        /// <para>Specifies that the value of this <see cref="Node"/> precedes the values of the given <paramref name="nodes" />.</para>
        /// <para>(i.e. This node's value must be determined first before the given <paramref name="nodes"/> values can be determined.)</para>
        /// </summary>
        /// <param name="nodes">
        /// The <see cref="Node"/>(s) that this node precedes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one of the given <paramref name="nodes"/> is null.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when one of the given <paramref name="nodes"/> is not valid for this node to precede.
        /// </exception>
        public void Precedes(params Node[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node is null)
                {
                    throw new ArgumentNullException(nameof(nodes), "Node was null.");
                }

                if (node.GetType() == this.GetType())
                {
                    throw new Exception(
                        $"A Node cannot precede a Node of the same Type ({node.GetType()})");
                }

                this.Followers.Add(node);
                node.Preceders.Add(this);
            }
        }

        /// <summary>
        /// <para>Specifies that the value of this <see cref="Node"/> follows (or "depends on") the values of the given <paramref name="nodes" />.</para>
        /// <para>(i.e. This node's value cannot be determined until the values of the given <paramref name="nodes"/> have been determined.)</para>
        /// </summary>
        /// <param name="nodes">
        /// The <see cref="Node"/>(s) that this node follows (i.e. the <see cref="Node"/>(s) containing values required to determine this node's value).
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when one of the given <paramref name="nodes"/> is null.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown when one of the given <paramref name="nodes"/> is not valid for this node to follow.
        /// </exception>
        public void Follows(params Node[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node is null)
                {
                    throw new ArgumentNullException(nameof(nodes), "Node was null.");
                }

                if (node.GetType() == this.GetType())
                {
                    throw new Exception(
                        $"A Node cannot follow a Node of the same Type ({node.GetType()})");
                }

                this.Preceders.Add(node);
                node.Followers.Add(this);
            }
        }

        /// <summary>
        /// Returns true if this node has followers (<see cref="Followers"/> contains at least one <see cref="Node"/>)
        /// </summary>
        /// <returns>
        /// true if this node has followers
        /// </returns>
        public bool HasFollowers()
        {
            return this.Followers.Any();
        }

        /// <summary>
        /// Returns true if this node has preceders (<see cref="Preceders"/> contains at least one <see cref="Node"/>)
        /// </summary>
        /// <returns>
        /// true if this node has preceders
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public bool HasPreceders()
        {
            return this.Preceders.Any();
        }

        /// <summary>
        /// Gets the most recent value of the <see cref="valueLedger"/>.
        /// </summary>
        /// <returns>
        /// The most recent value of the <see cref="valueLedger"/>
        /// </returns>
        public object GetValue()
        {
            return this.GetPreviousValue(0);
        }

        /// <summary>
        /// Gets the second-most recent value of the <see cref="valueLedger"/>.
        /// </summary>
        /// <returns>
        /// The second-most recent value of the <see cref="valueLedger"/>.
        /// </returns>
        public object GetPreviousValue()
        {
            return this.GetPreviousValue(1);
        }

        /// <summary>
        /// Gets the nth-most recent value of the <see cref="valueLedger"/>.
        /// </summary>
        /// <param name="age">
        /// How many "values ago" to get (0 for most recent - see <seealso cref="GetValue"/>).
        /// </param>
        /// <returns>
        /// The nth-most recent value of the <see cref="valueLedger"/>.
        /// </returns>
        public object GetPreviousValue(int age)
        {
            lock (this.valueLock)
            {
                return this.valueLedger.ElementAt(age);
            }
        }

        /// <summary>
        /// See <seealso cref="StatePropertyAttribute.Enabled"/>
        /// </summary>
        /// <returns>
        /// true if <see cref="StatePropertyAttribute.Enabled"/> is set to true for this node.
        /// false if <see cref="StatePropertyAttribute.Enabled"/> is set to false (or unspecified) for this node.
        /// </returns>
        public bool IsEnabled()
        {
            return this.GetStatePropertyAttribute()?.Enabled == true;
        }

        /// <summary>
        /// See <seealso cref="StatePropertyAttribute.StrictValue"/>
        /// </summary>
        /// <returns>
        /// true if <see cref="StatePropertyAttribute.StrictValue"/> is set to true for this node.
        /// false if <see cref="StatePropertyAttribute.StrictValue"/> is set to false (or unspecified) for this node.
        /// </returns>
        public bool IsStrictValue()
        {
            return this.GetStatePropertyAttribute()?.StrictValue == true;
        }

        /// <summary>
        /// Initializes a new empty <see cref="NodeCollection"/> and assigns it to <see cref="InitializedNodes"/>
        /// </summary>
        internal static void ResetInitializedNodes()
        {
            InitializedNodes = new NodeCollection();
        }

        /// <summary>
        /// Initializes a new empty set of <see cref="nodeStatePropertyNames"/>
        /// </summary>
        internal static void ResetNodeStatePropertyNames()
        {
            nodeStatePropertyNames = new HashSet<string>();
        }

        /// <summary>
        /// Called when the value of this node changes.
        /// </summary>
        /// <param name="value">
        /// The new value of this node.
        /// </param>
        internal void HandleValueChanged(object value)
        {
            this.OnValueChange(this, new NodeEventArgs(value));
        }

        /// <summary>
        /// Gets the <see cref="StatePropertyAttribute"/> for this node.
        /// </summary>
        /// <returns>
        /// The <see cref="StatePropertyAttribute"/> for this node..
        /// </returns>
        internal StatePropertyAttribute GetStatePropertyAttribute()
        {
            return (StatePropertyAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(StatePropertyAttribute));
        }

        /// <summary>
        /// <para>Inserts the given value <paramref name="value"/> at the beginning of the <see cref="valueLedger"/> (index 0).</para>
        /// <para>If the given value is null, set the beginning value to null without modifying any of the "previous" values (values that came before the most recent value).</para>
        /// </summary>
        /// <param name="value">
        /// The value to be added to the <see cref="valueLedger"/>.
        /// </param>
        /// <returns>
        /// true if <paramref name="value"/> was successfully added to <see cref="valueLedger"/>.
        /// </returns>
        internal bool SetValue(object value)
        {
            lock (this.valueLock)
            {
                if (value is null)
                {
                    this.valueLedger.AddFirst(new LinkedListNode<object>(null));
                }
                else
                {
                    this.valueLedger.AddFirst(value);
                }

                while (this.valueLedger.Count > this.valueLedgerCapacity)
                {
                    this.valueLedger.RemoveLast();
                }

                Logger.Debug("{0} Value set to '{1}' (ledger count {2}).", this.T(), value, this.valueLedger.Count);

                return true;
            }
        }

        /// <summary>
        /// Sets the first (most recent) value on the <see cref="valueLedger"/> to null without shifting it to the previous value.
        /// </summary>
        internal void NullifyValueWithoutShiftingToPrevious()
        {
            lock (this.valueLock)
            {
                var v = this.GetValue();

                if (v is null)
                {
                    return;
                }

                this.valueLedger.RemoveFirst();
                this.valueLedger.AddFirst(new LinkedListNode<object>(null));

                Logger.Debug($"{this.T()} Value NULLIFIED (prev value = {this.GetPreviousValue()})");
            }
        }

        /// <summary>
        /// <para>Validator of the <see cref="StatePropertyAttribute"/> name value.</para>
        /// <para>Valid if property is overridden and not defined as null/empty/whitespace.</para>
        /// </summary>
        /// <returns>
        /// <para>true if valid</para>
        /// <para>false if invalid</para>
        /// </returns>
        private bool IsValidStatePropertyName()
        {
            var statePropertyAttribute = this.GetStatePropertyAttribute();

            if (statePropertyAttribute?.Name is null)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(statePropertyAttribute.Name);
        }

        /// <summary>
        /// Attempts to add a <see cref="Node"/> to the list of <see cref="InitializedNodes"/>
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        private void AddInitializedNode(Node node)
        {
            Exception e = this.BuildExceptionForNodeBeingInitialized(node);

            if (e != null)
            {
                throw e;
            }

            var statePropertyAttribute = this.GetStatePropertyAttribute();

            if (statePropertyAttribute != null)
            {
                nodeStatePropertyNames.Add(statePropertyAttribute.Name);
            }

            InitializedNodes.Add(this);
        }

        /// <summary>
        /// Returns an <see cref="Exception"/> if the given <see cref="Node"/> is not valid for being added to <see cref="InitializedNodes"/>
        /// </summary>
        /// <param name="node">
        /// The <see cref="Node"/> being initialized.
        /// </param>
        /// <returns>
        /// An <see cref="Exception"/> if invalid.
        /// null if valid.
        /// </returns>
        private Exception BuildExceptionForNodeBeingInitialized(Node node)
        {
            // TODO: REFACTOR - Replace ArgumentException with custom Exception class for each potential invalid reason below (also replace in NodeTest.cs )
            if (InitializedNodes.Exists(node))
            {
                return new ArgumentException(
                    $"Node of type {node.GetType()} already initialized. Only one node of each node type is allowed.");
            }

            // StatePropertyName must be unique to each node
            if (!this.IsValidStatePropertyName())
            {
                return new ArgumentException(
                    $"StatePropertyName of node {node.GetType()} is invalid (must not be null, empty, or whitespace).");
            }

            var statePropertyAttribute = this.GetStatePropertyAttribute();

            if (statePropertyAttribute?.Name != null && nodeStatePropertyNames.Contains(statePropertyAttribute.Name))
            {
                return new ArgumentException(
                    $"Node {node.GetType()}: Another node already has a StatePropertyName of {statePropertyAttribute.Name}. Must be unique.");
            }

            return null;
        }

        /// <summary>
        /// Initializes <see cref="valueLedger"/> for this node.
        /// </summary>
        private void InitValueLedger()
        {
            lock (this.valueLock)
            {
                this.valueLedger = new LinkedList<object>();

                for (var i = 0; i < this.valueLedgerCapacity; i++)
                {
                    this.valueLedger.AddFirst(new LinkedListNode<object>(null));
                }
            }
        }

        /// <summary>
        ///     Set index 0 of ledger to null
        /// </summary>
        private void _ResetValue()
        {
            if (this.GetValue() != null) this.SetValue(null);
        }

        public bool ValueChanged()
        {
            var current = this.GetPreviousValue(0);
            var previous = this.GetPreviousValue(1);

            if (current is null)
                return false;

            if (current is null && previous is null)
                return false;

            return !current.Equals(previous);
        }

        // public void Dispose()
        // {
        // TaskManager.Dispose();
        // GC.SuppressFinalize(this);
        // return;
        // }
        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                    if (this.TaskManager != null)
                        this.TaskManager.Dispose();

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.
                this.valueLedger = null;

                this.disposedValue = true;
            }
        }

        // to do: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Node() {
        // // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        // Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);

            // to do: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion

#if DEBUG
        internal string T()
        {
            return this.GetType().ToString().PadRight(30);
        }

#else
        internal string T()
        {
            return this.GetType().ToString().PadRight(30);
        }
#endif
    }
}