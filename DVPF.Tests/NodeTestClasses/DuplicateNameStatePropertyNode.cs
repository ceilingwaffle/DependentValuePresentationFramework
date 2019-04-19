namespace DVPF.Tests.NodeTestClasses
{
    using System;
    using System.Threading.Tasks;

    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The duplicate name state property node.
    /// </summary>
    [StateProperty(enabled: true, name: "PropertyName")]
    internal class DuplicateNameStatePropertyNode : Node
    {
        /// <inheritdoc />
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }
}