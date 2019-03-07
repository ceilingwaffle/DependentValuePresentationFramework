using System.Threading.Tasks;
using RTSP.Core;

namespace RTSP.Tests.Nodes
{
    internal class LMC : Node
    {
        public override string StatePropertyName => "LMC";

        public override Task<object> DetermineValueAsync()
        {
            return Task.Run(() => { return new object(); });
        }
    }
}
