namespace DVPF.Tests.NodeTestClasses
{
    using System;
    using System.Threading.Tasks;

    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The null name state property node.
    /// </summary>
    [StateProperty(enabled: true, name: null)]
    internal class NullNameStatePropertyNode : Node
    {
        /// <inheritdoc />
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }
}