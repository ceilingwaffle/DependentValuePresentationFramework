using RTSP.Core;
using System.Threading.Tasks;

namespace RTSP.Tests.Nodes
{
    internal class Sun : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            return Task.Run(() => { return new object(); });
        }
    }
}
