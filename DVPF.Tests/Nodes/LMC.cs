namespace DVPF.Tests.Nodes
{
    using System.Threading.Tasks;
    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The Large Magellanic Cloud node.
    /// </summary>
    [StateProperty(enabled: true, name: "LMC")]
    // ReSharper disable once InconsistentNaming
    internal class LMC : Node
    {
        /// <inheritdoc />
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
