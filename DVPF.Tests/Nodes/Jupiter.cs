namespace DVPF.Tests.Nodes
{
    using System.Threading.Tasks;
    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The Jupiter node.
    /// </summary>
    [StateProperty(enabled: false, name: "Jupiter")]
    internal class Jupiter : Node
    {
        /// <inheritdoc />
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
