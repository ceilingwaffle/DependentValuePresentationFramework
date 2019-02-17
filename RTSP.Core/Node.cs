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
        private List<object> _valueLedger;

        private Task _updateTask;
        private TimeSpan _updateTimeLimit = TimeSpan.FromSeconds(30);

        public Dictionary<Type, Node> Children { get; }
        public Dictionary<Type, Node> Parents { get; }

        public Node()
        {
            _valueLedger = new List<object>(2);
            Children = new Dictionary<Type, Node>();
            Parents = new Dictionary<Type, Node>();
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

            Debug.WriteLine($"{T()}_updateTask.S = {_updateTask.Status.ToString()}.");

            this.DisposeUpdateTask();
        }

        private Task GetUpdateTask()
        {
                Debug.WriteLineIf(_updateTask != null, $"{T()}_updateTask already running.");

            if (_updateTask == null)
            {
                //_ResetValue();

                var cts = new CancellationTokenSource(_updateTimeLimit);

                _updateTask = Task.Run(async () =>
                {
                    Debug.WriteLine($"{T()} FetchData().", LogCategory.Event, this);
                    await Task.Delay(TimeSpan.FromMilliseconds(500));

                    if (_ValueChanged())
                    {
                        // TODO: Issue cancel to all children (if IsCancelled -> DisposeUpdateTask())
                        Debug.WriteLine($"{T()} Value changed: ({GetPreviousValue()} -> {GetValue()}).", LogCategory.ValueChanged, this);
                    }

                    var parents = Parents.ToList().Select((n) => { return n.Value; });
                    Parallel.ForEach(parents, async parent =>
                    {
                        await parent.GetUpdateTask();
                    });

                    Debug.WriteLine($"{T()} CalculateValue().");
                    _SetValue(Helpers.UnixTimestamp());

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
            Debug.WriteLine($"{T()} DisposeUpdateTask().");
            _updateTask = null;
        }

        /// <summary>
        /// Insert at index 0 on ledger.
        /// If the given value is null, set value at index 0 to null without modifying the previous value.
        /// </summary>
        /// <param name="v"></param>
        private void _SetValue(object v)
        {
            if (v == null)
            {
                _valueLedger.RemoveAt(0);
                _valueLedger.Insert(0, null);
                Debug.WriteLine($"{T()} Value set to NULL.");
                return;
            }

            _valueLedger.Insert(0, v);
            Debug.WriteLine($"{T()} Value set to {v}.");
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
            if (_valueLedger.Count <= age)
            {
                return default(object);
            }

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
