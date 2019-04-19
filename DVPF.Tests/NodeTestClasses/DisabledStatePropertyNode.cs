namespace DVPF.Tests.NodeTestClasses
{
    using System;
    using System.Threading.Tasks;

    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The disabled state property node.
    /// </summary>
    [StateProperty(enabled: false, name: "HopefullyNotEnabled")]
    internal class DisabledStatePropertyNode : Node
    {
        /// <inheritdoc />
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }
}