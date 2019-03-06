using RTSP.Core;
using System.Threading.Tasks;

namespace RTSP.Tests.Nodes
{
    class Andromeda : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            return Task.Run(() => { return new object(); });
        }
    }
}
