using System.Threading.Tasks;
using RTSP.Core;

namespace RTSP.Tests.Nodes
{
    class HalleysComet : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            return Task.Run(() => { return new object(); });
        }
    }
}
