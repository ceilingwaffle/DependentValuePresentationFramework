using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DVPF.Core
{
    internal class NodeTaskManager : IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The node being managed.
        /// </summary>
        private readonly Node _node;

        private Task _updateTask;
        private CancellationTokenSource _updateTaskCTS;

        /// <summary>
        /// key = Node Type, value = CTS
        /// </summary>
        private readonly Dictionary<Type, CancellationTokenSource> _followerTaskCTSList = new Dictionary<Type, CancellationTokenSource>();

        private readonly object _followerTaskCTSListLock = new object();

        // TODO: Load this from config
        private TimeSpan _updateTimeLimit = TimeSpan.FromMilliseconds(10000);

        public NodeTaskManager(Node node)
        {
            _node = node;
        }

        internal void ResetUpdateTaskCTS()
        {
            //if (_updateTaskCTS != null)
            //    _updateTaskCTS.Dispose();
            _updateTaskCTS = new CancellationTokenSource();

            foreach (var preceder in _node.Preceders)
            {
                preceder.TaskManager.AddFollowerCTS(_node.GetType(), _updateTaskCTS);
            }

            _logger.Debug($"{_node.T()} _ResetUpdateTaskCTS().");
        }

        internal void DisposeUpdateTask()
        {
            if (_updateTask != null)
            {
                //_updateTask.Dispose();
                _updateTask = null;
                _logger.Debug($"{_node.T()} DisposeUpdateTask().");
            }
        }

        private void AddFollowerCTS(Type followerNodeType, CancellationTokenSource updateTaskCTS)
        {
            _followerTaskCTSList[followerNodeType] = updateTaskCTS;
        }

        //internal async Task UpdateAsync()
        //{
        //    var updateTask = GetUpdateTask();

        //    //if (_updateTask != null && (_updateTask.IsCanceled || _updateTask.IsCompleted || _updateTask.IsFaulted))
        //    //{
        //    //    _logger.Debug($"{_node.T()} Resetting _updateTask (task status: {_updateTask.Status.ToString()})");

        //    //    DisposeUpdateTask();
        //    //    ResetUpdateTaskCTS();
        //    //}



        //    //updateTask = GetUpdateTask();



        //    //if (updateTask != null && updateTask.IsCompleted)
        //    //{
        //    //    _logger.Debug($"{_node.T()} task is already running.");
        //    //    return;
        //    //}

        //    //if (updateTask.IsCanceled)
        //    //{
        //    //    _logger.Debug($"{_node.T()} task is cancelled");
        //    //    return;
        //    //}



        //    await updateTask.ConfigureAwait(false);
        //}

        internal async Task UpdateAsync()
        {
            // TODO: Figure out when to check the CTS to not continue with the task. 

            try
            {
                //if (_updateTask != null)
                //{
                //    _logger.Debug("{0} _updateTask already running (_updateTask != null).", _node.T());
                //}

                //if (GetUpdateTaskStatus() == TaskStatus.Running)
                //{
                //    _logger.Debug($"{_node.T()} -----------UpdateAsync() ALREADY RUNNING.");
                //    return;
                //}

                _logger.Debug($"{_node.T()} UpdateAsync() START...");

                if (_updateTask?.Status == TaskStatus.Running)
                {
                    _logger.Debug("{0} _updateTask already running (_updateTask?.Status == TaskStatus.Running).", _node.T());
                }

                if (_updateTask is null)
                {
                    _updateTask = CreateUpdateTask();
                }

                if (!_updateTask.IsCanceled)
                {
                    try
                    {
                        await _updateTask;
                    }
                    catch (AggregateException ae)
                    {
                        _logger.Error(ae.Message);

                        foreach (var e in ae.InnerExceptions)
                        {
                            _logger.Error(e.Message);
                        }
                    }
                    catch (TaskCanceledException tce)
                    {
                        _logger.Error(tce.Message);
                    }
                    catch (OperationCanceledException oce)
                    {
                        _logger.Error(oce.Message);
                    }
                }

            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            
        }

        private async Task CreateUpdateTask()
        {
            try
            {
                _logger.Debug($"{_node.T()} Running Update Task...");

                // TODO: _updateTaskCts.CancelAfter(t)  <- read "t" from Class Attribute, configurable per node
                _updateTaskCTS.CancelAfter(_updateTimeLimit);

                if (_NullifyValueIfUpdateTaskIsCancelled())
                    return;

                //foreach (var preceder in _node.Preceders)
                //{
                //    // TODO: these should execute in parallel
                //    _logger.Debug($"{_node.T()} Requesting update from preceder: {preceder.GetType().ToString()}");
                //    await preceder.TaskManager.UpdateAsync().ConfigureAwait(false);
                //}

                await Task.WhenAll(_GetPrecederUpdateTasks());

                if (_NullifyValueIfUpdateTaskIsCancelled())
                    return;

                object value = null;

                value = await _node.DetermineValueAsync();

                if (_NullifyValueIfUpdateTaskIsCancelled())
                    return;

                _node.SetValue(value);
                _CancelFollowerTasksIfValueUpdated();

                _logger.Debug($"{_node.T()} Update Task completed: {_updateTask?.Status.ToString()}");
            }
            catch (AggregateException ae)
            {
                _logger.Error($"Caught Task Error: {ae.Message}");
            }

        }

        private bool _NullifyValueIfUpdateTaskIsCancelled()
        {
            if (!_updateTaskCTS.IsCancellationRequested)
                return false;

            _logger.Debug($"{_node.T()} Cancellation was requested. Resetting value to null. (a)");
            _node.NullifyValueWithoutShiftingToPrevious();

            return true;
        }

        /// <summary>
        /// This Node's value changed, meaning all follower node values are now
        /// potentially expired, so issue a cancel to all follower update tasks.
        /// </summary>
        private void _CancelFollowerTasksIfValueUpdated()
        {
            if (!_node.ValueChanged())
            {
                _logger.Debug($"{_node.T()} Value was same: ({_node.GetPreviousValue()} -> {_node.GetValue()}).");
            }
            else
            {
                _logger.Debug($"{_node.T()} Value changed: ({_node.GetPreviousValue()} -> {_node.GetValue()}).");

                HashSet<Node> toBeVisited = new HashSet<Node>();

                foreach (var follower in _node.Followers)
                    toBeVisited.Add(follower);

                while (toBeVisited.Count > 0)
                {
                    Node targetDescendent = toBeVisited.First();
                    toBeVisited.Remove(targetDescendent);

                    _logger.Debug($"{_node.T()} Issuing cancel to follower {targetDescendent.T()}...");
                    targetDescendent.TaskManager._CancelFollowerTasks();

                    // Check the StateAttribute of Node allowing toggle of "nullify value if any parent value changes", then conditionally check for it and run NullifyValueWithoutShiftingToPrevious() if true.
                    if (targetDescendent.IsStrictValue())
                    {
                        _logger.Debug($"{targetDescendent.T()} Strict value TRUE. Nullifying value...");
                        targetDescendent.NullifyValueWithoutShiftingToPrevious();

                    }

                    foreach (var follower in targetDescendent.Followers)
                        toBeVisited.Add(follower);

                    //_logger.Debug($"toBeVisited.Count: {toBeVisited.Count.ToString()}");
                }
            }
        }

        private void _CancelFollowerTasks()
        {
            // ToList is used to avoid error thrown in rare chance where a thread modifies the contents while another thread is enumerating
            foreach (var cts_kv in _node.TaskManager._followerTaskCTSList.ToList())
            {
                CancellationTokenSource cts = cts_kv.Value;

                if (cts is null)
                    return;

                cts.Cancel();
            }
        }

        private Task[] _GetPrecederUpdateTasks()
        {
            var preceders = _node.Preceders.ToArray();
            var precederTasks = new Task[_node.Preceders.Count()];

            for (int i = 0; i < _node.Preceders.Count(); i++)
            {
                var preceder = preceders[i];

                if (preceder is null)
                    continue;

                _logger.Debug($"{_node.T()} Requesting update from preceder: {preceder.GetType().ToString()}");
                precederTasks[i] = preceder.TaskManager.UpdateAsync();
            }

            return precederTasks;
        }

        internal TaskStatus GetUpdateTaskStatus()
        {
            if (_updateTask is null)
            {
                // TODO: Something else like a custom UpdateTaskStatus class which extends TaskStatus but has a custom null status.
                return TaskStatus.WaitingToRun;
            }

            return _updateTask.Status;
        }

        public void Dispose()
        {
            _updateTaskCTS.Dispose();
        }
    }
}
