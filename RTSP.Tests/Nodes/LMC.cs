using System.Threading.Tasks;
using RTSP.Core;

namespace RTSP.Tests.Nodes
{
    [StateProperty(enabled: true, name: "LMC")]
    internal class LMC : Node
    {
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
