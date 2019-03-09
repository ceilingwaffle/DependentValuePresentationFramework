using System.Threading.Tasks;
using RTSP.Core;

namespace RTSP.Tests.Nodes
{
    [StateProperty(enabled: true, name: "MilkyWay")]
    internal class MilkyWay : Node
    {
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
