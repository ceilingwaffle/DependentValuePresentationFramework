using System.Threading.Tasks;
using RTSP.Core;

namespace RTSP.Tests.Nodes
{
    internal class Oumuamua : Node
    {
        public override Task<object> DetermineValueAsync()
        {
            return Task.Run(() => { return new object(); });
        }
    }
}
