using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RTSP.Core
{
    internal class NodeTaskManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The node being managed.
        /// </summary>
        private readonly Node _node;

        private Task _updateTask;
        private CancellationTokenSource _updateTaskCTS;
        // TODO: Load this from config
        private TimeSpan _updateTimeLimit = TimeSpan.FromMilliseconds(10000);


        public NodeTaskManager(Node node)
        {
            _node = node;
        }

        internal void ResetUpdateTaskCTS()
        {
            if (_updateTaskCTS != null)
                _updateTaskCTS.Dispose();

            _updateTaskCTS = new CancellationTokenSource();
            _logger.Debug($"{_node.T()} _ResetUpdateTaskCTS().");
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
            _logger.Debug($"{_node.T()} UpdateAsync() START...");

            //if (_updateTask != null)
            //{
            //    _logger.Debug("{0} _updateTask already running (_updateTask != null).", _node.T());
            //}

            //if (GetUpdateTaskStatus() == TaskStatus.Running)
            //{
            //    _logger.Debug($"{_node.T()} -----------UpdateAsync() ALREADY RUNNING.");
            //    return;
            //}

            if (_updateTask?.Status == TaskStatus.Running)
            {
                _logger.Debug("{0} _updateTask already running (_updateTask?.Status == TaskStatus.Running).", _node.T());
            }

            if (_updateTask != null && (_updateTask.IsCanceled || _updateTask.IsCompleted || _updateTask.IsFaulted))
            {
                _logger.Debug($"{_node.T()} Resetting _updateTask (task status: {_updateTask.Status.ToString()})");

                DisposeUpdateTask();
                ResetUpdateTaskCTS();
            }

            if (_updateTask == null)
            {
                _updateTask = GetUpdateTask();
            }

            await _updateTask.ConfigureAwait(false);
        }

        private Task GetUpdateTask()
        {
            return Task.Run(async () =>
            {
                if (_updateTaskCTS.IsCancellationRequested)
                {
                    _logger.Debug($"{_node.T()} Cancellation was requested. Not continuing with calc/data fetching.");
                    return;
                }

                foreach (var preceder in _node.Preceders)
                {
                    _logger.Debug($"{_node.T()} requesting update from preceder: {preceder.GetType().ToString()}");
                    await preceder.TaskManager.UpdateAsync().ConfigureAwait(false);
                }

                //Task.WaitAll(_GetPrecederUpdateTasks());

                // TODO: calculate value
                var value = await _node.DetermineValueAsync();
                _node.SetValue(value);

                // TODO: Need to cancel all followers of followers also (not just direct followers)
                _CancelFollowerTasksIfValueUpdated();

                _logger.Debug($"{_node.T()}updateTask completed.");

            }, _updateTaskCTS.Token);

            //return Task.Run(async () =>
            //{
            //    if (_updateTaskCTS.IsCancellationRequested)
            //    {
            //        _logger.Debug($"{_node.T()} Cancellation was requested. Not continuing with calc/data fetching.");
            //        return;
            //    }

            //    foreach (var preceder in _node.Preceders)
            //    {
            //        _logger.Debug($"{_node.T()} requesting update from preceder: {preceder.GetType().ToString()}");
            //        await preceder.TaskManager.UpdateAsync();
            //    }

            //    //Task.WaitAll(_GetPrecederUpdateTasks());

            //    // TODO: calculate value
            //    var value = await _node.DetermineValueAsync();
            //    _node.SetValue(value);

            //    // TODO: Need to cancel all children of children also (not just direct children)
            //    _CancelFollowerTasksIfValueUpdated();

            //    _logger.Debug($"{_node.T()}updateTask completed.");

            //}, _updateTaskCTS.Token);
        }

        private void _CancelFollowerTasksIfValueUpdated()
        {
            if (!_node.ValueChanged())
            {
                _logger.Debug($"{_node.T()} Value was same: ({_node.GetPreviousValue()} -> {_node.GetValue()}).");
            }
            else
            {
                _logger.Debug($"{_node.T()} Value changed: ({_node.GetPreviousValue()} -> {_node.GetValue()}).");

                // This Node's value changed, meaning all follower node values are now potentially expired.
                // So issue a cancel to all follower update tasks.


                //foreach (var follower in _node.Followers)
                //{
                //    //if (follower.TaskManager.GetUpdateTaskStatus() == TaskStatus.Running)
                //    //{
                //    //    _logger.Debug($"{_node.T()} Issuing cancel to running follower {follower.T()}...");
                //    //    follower.TaskManager._updateTaskCTS.Cancel();
                //    //}

                //    Node f = follower;

                //    f.TaskManager.DisposeUpdateTask();
                //    f.NullifyValueWithoutShiftingToPrevious();

                //}

                HashSet<Node> toBeVisited = new HashSet<Node>();

                foreach (var follower in _node.Followers)
                {
                    toBeVisited.Add(follower);
                }

                while (toBeVisited.Count > 0)
                {
                    Node targetDescendent = toBeVisited.First();
                    toBeVisited.Remove(targetDescendent);

                    _logger.Debug($"{_node.T()} Issuing cancel to follower {targetDescendent.T()}...");
                    targetDescendent.TaskManager.DisposeUpdateTask();
                    targetDescendent.TaskManager.ResetUpdateTaskCTS();
                    //targetDescendent.TaskManager.UpdateAsync().ConfigureAwait(false);

                    //target.NullifyValueWithoutShiftingToPrevious();

                    foreach (var follower in targetDescendent.Followers)
                    {
                        toBeVisited.Add(follower);
                    }

                    _logger.Debug($"toBeVisited.Count: {toBeVisited.Count.ToString()}");
                }


            }
        }

        //private Task[] _GetPrecederUpdateTasks()
        //{
        //    var preceders = _node.Preceders.ToArray();
        //    var precederTasks = new Task[_node.Preceders.Count()];

        //    for (int i = 0; i < _node.Preceders.Count(); i++)
        //    {
        //        var preceder = preceders[i];

        //        if (preceder is null)
        //            continue;

        //        precederTasks[i] = preceder.TaskManager.UpdateAsync().ConfigureAwait(false);
        //    }

        //    return precederTasks;
        //}

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
                _logger.Debug($"{_node.T()} DisposeUpdateTask().");
            }
        }

    }
}
