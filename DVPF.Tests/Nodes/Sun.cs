namespace DVPF.Tests.Nodes
{
    using System.Threading.Tasks;
    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The sun.
    /// </summary>
    [StateProperty(enabled: false, name: "Sun")]
    internal class Sun : Node
    {
        /// <inheritdoc />
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
