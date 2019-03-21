using System;
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

        internal async Task UpdateAsync()
        {
            var updateTask = GetUpdateTask();

            if (updateTask.IsCanceled)
            {
                _logger.Debug($"{_node.T()} task is cancelled");
                return;
            }

            await updateTask.ConfigureAwait(false);

            if (_updateTask != null && (_updateTask.IsCanceled || _updateTask.IsCompleted || _updateTask.IsFaulted))
            {
                _logger.Debug($"{_node.T()} task status: {_updateTask.Status.ToString()}");

                _DisposeUpdateTask();
                ResetUpdateTaskCTS();
            }
        }

        private Task GetUpdateTask()
        {
            if (_updateTask != null)
            {
                _logger.Debug("{0} _updateTask already running (_updateTask != null).", _node.T());
                //Debug.WriteLineIf(_updateTask != null,
                //    $"{_node.T()}_updateTask already running (_updateTask != null).");
            }

            if (_updateTask?.Status == TaskStatus.Running)
            {
                _logger.Debug("{0} _updateTask already running (_updateTask?.Status == TaskStatus.Running).", _node.T());

                //Debug.WriteLineIf(_updateTask?.Status == TaskStatus.Running,
                //    $"{_node.T()}_updateTask already running (_updateTask?.Status == TaskStatus.Running).");
            }

            if (_updateTask == null)
            {
                _updateTask = Task.Run(async () =>
                {
                    if (_updateTaskCTS.IsCancellationRequested)
                    {
                        _logger.Debug($"{_node.T()} Cancellation was requested. Not continuing with calc/data fetching.", LogCategory.Event, this);
                        return;
                    }

                    foreach (var parent in _node.Parents)
                    {
                        _logger.Debug($"{_node.T()} requesting update from parent: {parent.GetType().ToString()}");
                        await parent.TaskManager.UpdateAsync();
                    }

                    //Task.WaitAll(_GetParentUpdateTasks());

                    // TODO: calculate value
                    var value = await _node.DetermineValueAsync();
                    _node.SetValue(value);

                    // TODO: Need to cancel all children of children also (not just direct children)
                    _CancelChildTasksIfValueUpdated();

                    _logger.Debug($"{_node.T()}updateTask completed.");

                }, _updateTaskCTS.Token);
            }

            return _updateTask;
        }

        private void _CancelChildTasksIfValueUpdated()
        {
            if (_node.ValueChanged())
            {
                // This Node's value changed, meaning all child node values are now potentially expired.
                // So issue a cancel to all child update tasks.
                foreach (var child in _node.Children)
                {
                    _logger.Debug($"{_node.T()} Issuing cancel to child {child.T()}...");
                    child.TaskManager._updateTaskCTS.Cancel();
                }

                _logger.Debug($"{_node.T()} Value changed: ({_node.GetPreviousValue()} -> {_node.GetValue()}).", LogCategory.ValueChanged, this);
            }
            else
            {
                _logger.Debug($"{_node.T()} Value was same: ({_node.GetPreviousValue()} -> {_node.GetValue()}).", LogCategory.ValueChanged, this);
            }
        }

        private Task[] _GetParentUpdateTasks()
        {
            var parents = _node.Parents.ToArray();
            var parentTasks = new Task[_node.Parents.Count()];

            for (int i = 0; i < _node.Parents.Count(); i++)
            {
                var parent = parents[i];

                if (parent is null)
                    continue;

                parentTasks[i] = parent.TaskManager.UpdateAsync();
            }

            return parentTasks;
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
                _logger.Debug($"{_node.T()} _DisposeUpdateTask().");
            }
        }

    }
}
