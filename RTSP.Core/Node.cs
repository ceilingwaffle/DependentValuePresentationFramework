using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RTSP.Core
{
    public abstract class Node : IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly object _valueLock = new object();
        private LinkedList<object> _valueLedger;
        internal readonly NodeTaskManager TaskManager;

        public Node()
        {
            TaskManager = new NodeTaskManager(this);

            TaskManager.ResetUpdateTaskCTS();

            _InitValueLedger();

            _AddInitializedNode(this);
        }

        private static HashSet<string> NodeStatePropertyNames { get; set; } = new HashSet<string>();

        public NodeCollection Followers { get; } = new NodeCollection();

        public NodeCollection Preceders { get; } = new NodeCollection();

        internal static NodeCollection InitializedNodes { get; private set; } = new NodeCollection();

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

        public void Precedes(params Node[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node is null)
                    throw new ArgumentNullException("Node was null.");

                if (node.GetType() == this.GetType())
                    throw new ArgumentNullException($"A Node cannot precede a Node of the same Type ({node.GetType().ToString()})");

                Followers.Add(node);
                node.Preceders.Add(this);
            }
        }

        public void Follows(params Node[] nodes)
        {
            foreach (var node in nodes)
            {
                if (node is null)
                    throw new ArgumentNullException("Node was null.");

                if (node.GetType() == this.GetType())
                    throw new ArgumentNullException($"A Node cannot follow a Node of the same Type ({node.GetType().ToString()})");

                Preceders.Add(node);
                node.Followers.Add(this);
            }
        }

        public bool HasFollowers()
        {
            return Followers.Count() > 0;
        }

        public bool HasPreceders()
        {
            return Preceders.Count() > 0;
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

                if (v is null)
                {
                    _valueLedger.AddFirst(new LinkedListNode<object>(null));
                    //_logger.Debug($"{T()} Value set to NULL.");
                    //return false;
                }
                else
                {
                    _valueLedger.AddFirst(v);
                }

                while (_valueLedger.Count > capacity)
                    _valueLedger.RemoveLast();

                _logger.Debug("{0} Value set to '{1}' (ledger count {2}).", T(), v, _valueLedger.Count);

                return true;
            }
        }

        internal void NullifyValueWithoutShiftingToPrevious()
        {
            lock (_valueLock)
            {
                var v = GetValue();

                if (v is null)
                    return;

                _valueLedger.RemoveFirst();
                _valueLedger.AddFirst(new LinkedListNode<object>(null));

                _logger.Debug($"{T()} Value NULLIFIED (prev value = {GetPreviousValue()})");
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

            if (current is null)
                return false;

            if (current is null && previous is null)
                return false;

            return !current.Equals(previous);
        }

        //public void Dispose()
        //{
        //    TaskManager.Dispose();
        //    GC.SuppressFinalize(this);
        //    return;
        //}

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    if (TaskManager != null)
                    {
                        TaskManager.Dispose();
                    }
                }

                // free unmanaged resources (unmanaged objects) and override a finalizer below.
                // set large fields to null.
                _valueLedger = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Node() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
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
