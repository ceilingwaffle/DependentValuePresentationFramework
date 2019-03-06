using RTSP.Core;
using System.Threading.Tasks;

namespace RTSP.Tests.Nodes
{
    class Sun : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            return Task.Run(() => { return new object(); });
        }
    }
}
