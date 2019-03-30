using System.Threading.Tasks;
using DVPF.Core;

namespace DVPF.Tests.Nodes
{
    [StateProperty(enabled: false, name: "Oumuamua")]
    internal class Oumuamua : Node
    {
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
