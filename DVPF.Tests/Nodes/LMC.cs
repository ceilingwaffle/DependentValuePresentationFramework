using System.Threading.Tasks;
using DVPF.Core;

namespace DVPF.Tests.Nodes
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
