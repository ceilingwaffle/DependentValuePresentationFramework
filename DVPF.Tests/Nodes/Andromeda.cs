using DVPF.Core;
using System.Threading.Tasks;

namespace DVPF.Tests.Nodes
{
    [StateProperty(enabled: false, name: "Andromeda")]
    internal class Andromeda : Node
    {
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
