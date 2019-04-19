namespace DVPF.Tests.Nodes
{
    using System.Threading.Tasks;
    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The solar system node.
    /// </summary>
    [StateProperty(enabled: false, name: "SolarSystem")]
    internal class SolarSystem : Node
    {
        /// <inheritdoc />
        public override async Task<object> DetermineValueAsync()
        {
            return await Task.FromResult(new object());
        }
    }
}
