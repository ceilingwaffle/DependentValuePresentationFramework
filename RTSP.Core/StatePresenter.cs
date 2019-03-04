﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RTSP.Core
{
    public class StatePresenter
    {
        public NodeSupervisor NodeSupervisor { get; private set; }
        private TimeSpan _scannerInterval = TimeSpan.FromMilliseconds(500);

        public StatePresenter()
        {
            NodeSupervisor = new NodeSupervisor();
        }

        public async Task StartAsync()
        {
            NodeSupervisor.BuildNodeCollections();

            var cts = new CancellationTokenSource();

            while (true)
            {
                Debug.WriteLine("------------------------------------------------------");

                if (cts.IsCancellationRequested)
                    return;

                var leafNodes = NodeSupervisor.LeafNodes.ToEnumerable();

                foreach (var node in leafNodes)
                {
                    if (node.GetUpdateTaskStatus() != TaskStatus.Running)
                    {
                        Debug.WriteLine($"{node.T()} UpdateAsync() START...");
                        node.UpdateAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Debug.WriteLine($"{node.T()} -----------UpdateAsync() ALREADY RUNNING.");
                    }

                }

                await Task.Delay(_scannerInterval);
            }

        }

        public void ResetAllNodes()
        {
            Node.ResetInitializedNodes();
        }
    }
}
