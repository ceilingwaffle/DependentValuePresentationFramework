namespace DVPF.Tests.NodeTestClasses
{
    using System;
    using System.Threading.Tasks;

    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The valid enabled state property node.
    /// </summary>
    [StateProperty(enabled: true, name: "PropertyName")]
    internal class ValidEnabledStatePropertyNode : Node
    {
        /// <inheritdoc />
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }
}