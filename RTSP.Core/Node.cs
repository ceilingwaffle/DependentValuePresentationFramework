using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RTSP.Core
{
    // TODO: Extract some code into NodeValidator
    // TODO: Extract some code into NodeUpdater
    public abstract class Node
    {
        private readonly object _valueLock = new object();
        private LinkedList<object> _valueLedger;


        /// <summary>
        /// The property name of this node's value on the State object.
        /// Will only be included on the State if this is overriden and is valid (see _IsValidStatePropertyName method)
        /// </summary>
        //public virtual string StatePropertyName { get; private set; } = null; //TODO: This should be protected, but it breaks TestEnabledNodes

        private static HashSet<string> NodeStatePropertyNames { get; set; } = new HashSet<string>();

        public NodeCollection Children { get; } = new NodeCollection();
        public NodeCollection Parents { get; } = new NodeCollection();
        internal static NodeCollection InitializedNodes { get; private set; } = new NodeCollection();

        public static object tempValue;

        internal readonly NodeTaskManager TaskManager;

        public Node()
        {
            TaskManager = new NodeTaskManager(this);

            TaskManager.ResetUpdateTaskCTS();

            _InitValueLedger();
            _AddInitializedNode(this);
        }

        public abstract Task<object> DetermineValueAsync();

        /// <summary>
        /// Valid if property is overridden and not defined as null/empty/whitespace.
        /// </summary>
        /// <returns></returns>
        private bool _IsValidStatePropertyName()
        {
            StatePropertyAttribute statePropertyAttribute = GetStatePropertyAttribute();

            if (statePropertyAttribute?.Name is null)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(statePropertyAttribute.Name);
        }

        internal StatePropertyAttribute GetStatePropertyAttribute()
        {
            return (StatePropertyAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(StatePropertyAttribute));
        }

        private void _AddInitializedNode(Node node)
        {
            _ValidateNode(node);

            StatePropertyAttribute statePropertyAttribute = GetStatePropertyAttribute();

            if (statePropertyAttribute != null)
            {
                NodeStatePropertyNames.Add(statePropertyAttribute.Name);
            }

            InitializedNodes.Add(this);
        }

        private void _ValidateNode(Node node)
        {
            // TODO: Replace ArgumentException with custom Exception class for each potential invalid reason below (also replace in NodeTest.cs )

            if (InitializedNodes.Exists(node))
                throw new ArgumentException($"Node of type {node.GetType().ToString()} already initialized. Only one node of each node type is allowed.");

            // StatePropertyName must be unique to each node
            if (!_IsValidStatePropertyName())
                throw new ArgumentException($"StatePropertyName of node {node.GetType().ToString()} is invalid (must not be null, empty, or whitespace).");

            StatePropertyAttribute statePropertyAttribute = GetStatePropertyAttribute();

            if (statePropertyAttribute?.Name != null && NodeStatePropertyNames.Contains(statePropertyAttribute?.Name))
                throw new ArgumentException($"Node {node.GetType().ToString()}: Another node already has a StatePropertyName of {statePropertyAttribute?.Name}. Must be unique.");
        }

        internal static void ResetInitializedNodes()
        {
            InitializedNodes = new NodeCollection();
        }

        internal static void ResetNodeStatePropertyNames()
        {
            NodeStatePropertyNames = new HashSet<string>();
        }

        private void _InitValueLedger()
        {
            var capacity = 2;

            _valueLedger = new LinkedList<object>();

            for (var i = 0; i < capacity; i++)
                _valueLedger.AddFirst(new LinkedListNode<object>(null));
        }

        public void AddChildren(params Node[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node == null)
                    throw new ArgumentNullException(node.GetType().ToString());

                Children.Add(node);
                node.Parents.Add(this);
            }
        }

        public bool HasChildren()
        {
            return Children.Count() > 0;
        }

        public bool HasParents()
        {
            return Parents.Count() > 0;
        }

        /// <summary>
        /// Insert at index 0 on ledger.
        /// If the given value is null, set value at index 0 to null without modifying the previous value.
        /// </summary>
        /// <param name="v"></param>
        internal bool SetValue(object v)
        {
            lock (_valueLock)
            {
                var capacity = 2;

                if (v == null)
                {
                    _valueLedger.AddFirst(new LinkedListNode<object>(null));
                    Debug.WriteLine($"{T()} Value set to NULL.");
                    return false;
                }

                _valueLedger.AddFirst(v);

                while (_valueLedger.Count > capacity)
                    _valueLedger.RemoveLast();

                Debug.WriteLine($"{T()} Value set to {v} (ledger count {_valueLedger.Count}).");
                return true;
            }

        }

        public object GetValue()
        {
            return GetPreviousValue(age: 0);
        }

        public object GetPreviousValue()
        {
            return GetPreviousValue(age: 1);
        }

        public bool IsEnabled()
        {
            return GetStatePropertyAttribute()?.Enabled == true;
        }

        internal object GetPreviousValue(int age)
        {
            lock (_valueLock)
            {
                return _valueLedger.ElementAt(age);
            }

        }

        /// <summary>
        /// Set index 0 of ledger to null
        /// </summary>
        private void _ResetValue()
        {
            if (GetValue() != null)
            {
                SetValue(null);
            }
        }

        internal bool ValueChanged()
        {
            var current = GetPreviousValue(age: 0);
            var previous = GetPreviousValue(age: 1);

            if (current == null)
                return false;

            if (current == null && previous == null)
                return false;

            return !current.Equals(previous);
        }

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
