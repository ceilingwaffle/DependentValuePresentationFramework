using DVPF.Core;
using System.Threading.Tasks;

namespace DVPF.Tests.Nodes
{
    [StateProperty(enabled: false, name: "Sun")]
    internal class Sun : Node
    {
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
