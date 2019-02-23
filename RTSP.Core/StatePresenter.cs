using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTSP.Core
{
    public class StatePresenter
    {
        public NodeSupervisor NodeSupervisor { get; private set; }
        private TimeSpan _scannerInterval = TimeSpan.FromMilliseconds(1000);

        public StatePresenter()
        {
            NodeSupervisor = new NodeSupervisor();
        }

        public async Task StartAsync()
        {
            NodeSupervisor.BuildLeafNodes();

            var cts = new CancellationTokenSource();

            while (true)
            {
                Debug.WriteLine("------------------------------------------------------");

                if (cts.IsCancellationRequested)
                    return;

                var leafNodes = NodeSupervisor.LeafNodes.ToList().Select((n) => { return n.Value; });

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
    }
}
