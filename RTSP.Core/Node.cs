using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTSP.Core
{
    public abstract class Node
    {
        private readonly object valueLock = new object();
        private LinkedList<object> _valueLedger;

        private Task _updateTask;
        private TimeSpan _updateTimeLimit = TimeSpan.FromSeconds(30);

        public Dictionary<Type, Node> Children { get; }
        public Dictionary<Type, Node> Parents { get; }

        public Node()
        {
            _InitValueLedger();

            Children = new Dictionary<Type, Node>();
            Parents = new Dictionary<Type, Node>();
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

                Children[node.GetType()] = node;
                node.Parents[this.GetType()] = this;
            }
        }

        internal bool HasChildren()
        {
            return Children.Count > 0;
        }

        internal bool HasParents()
        {
            return Parents.Count > 0;
        }

        internal async Task UpdateAsync()
        {
            await GetUpdateTask();

            this.DisposeUpdateTask();
        }

        private Task GetUpdateTask()
        {
            Debug.WriteLineIf(
                _updateTask != null || _updateTask?.Status == TaskStatus.Running,
                $"{T()}_updateTask already running.");

            if (_updateTask == null)
            {
                //_ResetValue();

                var cts = new CancellationTokenSource(_updateTimeLimit);

                _updateTask = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(800));
                    var fetchedDataTs = Helpers.UnixTimestamp();
                    Debug.WriteLine($"{T()} Completed: FetchData().", LogCategory.Event, this);

                    var parents = Parents.ToList().Select((n) => { return n.Value; });
                    foreach (var parent in parents)
                    {
                        await parent.UpdateAsync();
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    var calculatedValue = Helpers.UnixTimestamp() - fetchedDataTs;
                    //_SetValue(calculatedValue);
                    _SetValue(Helpers.Rand(1,2));
                    Debug.WriteLine($"{T()} Completed: CalculateValue(fetchedData).", LogCategory.Event, this);

                    if (_ValueChanged())
                    {
                        // TODO: Issue cancel to all children (if IsCancelled -> DisposeUpdateTask())
                        Debug.WriteLine($"{T()} Value changed: ({GetPreviousValue()} -> {GetValue()}).", LogCategory.ValueChanged, this);
                    }
                    else
                    {
                        Debug.WriteLine($"{T()} Value was same: ({GetPreviousValue()} -> {GetValue()}).", LogCategory.ValueChanged, this);
                    }

                }, cts.Token);
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

        internal void DisposeUpdateTask()
        {
            if (_updateTask != null)
            {
                _updateTask = null;
                Debug.WriteLine($"{T()} DisposeUpdateTask().");
            }
        }

        /// <summary>
        /// Insert at index 0 on ledger.
        /// If the given value is null, set value at index 0 to null without modifying the previous value.
        /// </summary>
        /// <param name="v"></param>
        private bool _SetValue(object v)
        {
            lock (valueLock)
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
#endif
    }
}
