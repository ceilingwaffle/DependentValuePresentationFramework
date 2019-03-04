using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RTSP.Core
{
    public abstract class Node
    {
        private readonly object _valueLock = new object();
        private LinkedList<object> _valueLedger;

        private Task _updateTask;
        private CancellationTokenSource _updateTaskCTS;
        private TimeSpan _updateTimeLimit = TimeSpan.FromMilliseconds(10000);

        public NodeCollection Children { get; } = new NodeCollection();
        public NodeCollection Parents { get; } = new NodeCollection();
        internal static NodeCollection InitializedNodes { get; private set; } = new NodeCollection();

        public Node()
        {
            _ResetUpdateTaskCTS();
            _InitValueLedger();
            _AddInitializedNode(this);
        }

        private void _AddInitializedNode(Node node)
        {
            if (InitializedNodes.Exists(node))
                throw new ArgumentException("Node of this type already initialized. Only one Node of each Node Type is allowed.");

            InitializedNodes.Add(this);
        }

        internal static void ResetInitializedNodes()
        {
            InitializedNodes = new NodeCollection();
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

                    // fetch data
                    Debug.WriteLine($"{T()} Fetching data...", LogCategory.Event, this);
                    await Task.Delay(TimeSpan.FromMilliseconds(800));
                    var fetchedDataTs = Helpers.UnixTimestamp();
                    Debug.WriteLine($"{T()} Completed: FetchData().", LogCategory.Event, this);

                    var parents = Parents.ToEnumerable();

                    foreach (var parent in parents)
                    {
                        await parent.UpdateAsync().ConfigureAwait(false);
                    }

                    // calculate value
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    var calculatedValue = Helpers.UnixTimestamp() - fetchedDataTs;
                    //_SetValue(calculatedValue);
                    _SetValue(Helpers.Rand(1, 2));
                    Debug.WriteLine($"{T()} Completed: CalculateValue(fetchedData).", LogCategory.Event, this);

                    if (_ValueChanged())
                    {
                        // This Node's value changed, meaning all child node values are now potentially expired.
                        // So issue a cancel to all child update tasks.
                        var children = Children.ToEnumerable();
                        foreach (var child in children)
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

        internal object GetPreviousValue(int age)
        {
            return _valueLedger.ElementAt(age);
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
