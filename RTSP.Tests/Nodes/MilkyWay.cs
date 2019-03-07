using System.Threading.Tasks;
using RTSP.Core;

namespace RTSP.Tests.Nodes
{
    internal class MilkyWay : Node
    {
        public override string StatePropertyName => "MilkyWay";

        public override Task<object> DetermineValueAsync()
        {
            return Task.Run(() => { return new object(); });
        }
    }
}
