using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RTSP.Core
{
    // TODO: Extract some code into NodeValidator
    // TODO: Extract some code into NodeUpdater
    public abstract class Node
    {
        private readonly object _valueLock = new object();
        private LinkedList<object> _valueLedger;

        private Task _updateTask;
        private CancellationTokenSource _updateTaskCTS;
        // TODO: Load this from config
        private TimeSpan _updateTimeLimit = TimeSpan.FromMilliseconds(10000);

        /// <summary>
        /// The property name of this node's value on the State object.
        /// Will only be included on the State if this is overriden and is valid (see _IsValidStatePropertyName method)
        /// </summary>
        public virtual string StatePropertyName { get; private set; } = null; //TODO: This should be protected, but it breaks TestEnabledNodes

        private static HashSet<string> NodeStatePropertyNames { get; set; } = new HashSet<string>();

        public NodeCollection Children { get; } = new NodeCollection();
        public NodeCollection Parents { get; } = new NodeCollection();
        internal static NodeCollection InitializedNodes { get; private set; } = new NodeCollection();

        public static object tempValue;

        public Node()
        {
            _ResetUpdateTaskCTS();
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
            if (StatePropertyName is null)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(StatePropertyName);
        }

        private void _AddInitializedNode(Node node)
        {
            _ValidateNode(node);

            NodeStatePropertyNames.Add(StatePropertyName);
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

            if (StatePropertyName != null && NodeStatePropertyNames.Contains(StatePropertyName))
                throw new ArgumentException($"Node {node.GetType().ToString()}: Another node already has a StatePropertyName of {StatePropertyName}. Must be unique.");
        }

        internal static void ResetInitializedNodes()
        {
            InitializedNodes = new NodeCollection();
        }

        internal static void ResetNodeStatePropertyNames()
        {
            NodeStatePropertyNames = new HashSet<string>();
        }

        private void _ResetUpdateTaskCTS()
        {
            if (_updateTaskCTS != null)
                _updateTaskCTS.Dispose();

            _updateTaskCTS = new CancellationTokenSource();
            Debug.WriteLine($"{T()} _ResetUpdateTaskCTS().");
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

        internal async Task UpdateAsync()
        {
            var updateTask = GetUpdateTask();

            if (updateTask.IsCanceled)
            {
                Debug.WriteLine($"{T()} task is cancelled");
                return;
            }

            await updateTask.ConfigureAwait(false);

            if (_updateTask != null && (_updateTask.IsCanceled || _updateTask.IsCompleted || _updateTask.IsFaulted))
            {
                _DisposeUpdateTask();
                _ResetUpdateTaskCTS();
            }
        }

        private Task GetUpdateTask()
        {
            Debug.WriteLineIf(_updateTask != null,
                $"{T()}_updateTask already running (_updateTask != null).");

            Debug.WriteLineIf(_updateTask?.Status == TaskStatus.Running,
                $"{T()}_updateTask already running (_updateTask?.Status == TaskStatus.Running).");

            if (_updateTask == null)
            {
                _updateTask = Task.Run(async () =>
                {
                    if (_updateTaskCTS.IsCancellationRequested)
                    {
                        Debug.WriteLine($"{T()} Cancellation was requested. Not continuing with calc/data fetching.", LogCategory.Event, this);
                        return;
                    }

                    foreach (var parent in Parents)
                    {
                        await parent.UpdateAsync().ConfigureAwait(false);
                    }


                    // TODO: calculate value
                    var value = await DetermineValueAsync();
                    _SetValue(value);

                    if (_ValueChanged())
                    {
                        // This Node's value changed, meaning all child node values are now potentially expired.
                        // So issue a cancel to all child update tasks.
                        foreach (var child in Children)
                        {
                            Debug.WriteLine($"{T()} Issuing cancel to child {child.T()}...");
                            child._updateTaskCTS.Cancel();
                        }

                        Debug.WriteLine($"{T()} Value changed: ({GetPreviousValue()} -> {GetValue()}).", LogCategory.ValueChanged, this);
                    }
                    else
                    {
                        Debug.WriteLine($"{T()} Value was same: ({GetPreviousValue()} -> {GetValue()}).", LogCategory.ValueChanged, this);
                    }

                }, _updateTaskCTS.Token);
            }

            return _updateTask;
        }

        internal TaskStatus GetUpdateTaskStatus()
        {
            if (_updateTask == null)
            {
                // TODO: Something else like a custom UpdateTaskStatus class which extends TaskStatus but has a custom null status.
                return TaskStatus.WaitingToRun;
            }

            return _updateTask.Status;
        }

        private void _DisposeUpdateTask()
        {
            if (_updateTask != null)
            {
                _updateTask = null;
                Debug.WriteLine($"{T()} _DisposeUpdateTask().");
            }
        }

        /// <summary>
        /// Insert at index 0 on ledger.
        /// If the given value is null, set value at index 0 to null without modifying the previous value.
        /// </summary>
        /// <param name="v"></param>
        private bool _SetValue(object v)
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
            // TODO: make a comment on HasOver.. that if it's overridden but set to null, we define this as not overridden.
            //return Helpers.HasOverriddenProperty(this.GetType(), "StatePropertyName");

            return this.StatePropertyName != null;
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
                _SetValue(null);
            }
        }

        private bool _ValueChanged()
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
