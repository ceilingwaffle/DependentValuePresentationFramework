namespace DVPF.Tests.Nodes
{
    using System.Threading.Tasks;
    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The Milky Way galaxy node.
    /// </summary>
    [StateProperty(enabled: true, name: "MilkyWay")]
    internal class MilkyWay : Node
    {
        /// <inheritdoc />
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
