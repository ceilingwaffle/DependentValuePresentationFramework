using System.Threading.Tasks;
using RTSP.Core;

namespace RTSP.Tests.Nodes
{
    [StateProperty(enabled: false, name: "SolarSystem")]
    internal class SolarSystem : Node
    {
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
