using System.Threading.Tasks;
using RTSP.Core;

namespace RTSP.Tests.Nodes
{
    class LMC : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            return Task.Run(() => { return new object(); });
        }
    }
}
