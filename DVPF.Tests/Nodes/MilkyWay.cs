using System.Threading.Tasks;
using DVPF.Core;

namespace DVPF.Tests.Nodes
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
