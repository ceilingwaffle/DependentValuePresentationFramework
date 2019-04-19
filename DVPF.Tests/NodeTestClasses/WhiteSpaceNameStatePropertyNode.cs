namespace DVPF.Tests.NodeTestClasses
{
    using System;
    using System.Threading.Tasks;

    using DVPF.Core;

    /// <inheritdoc />
    /// <summary>
    /// The white space name state property node.
    /// </summary>
    [StateProperty(enabled: true, name: "  ")]
    internal class WhiteSpaceNameStatePropertyNode : Node
    {
        /// <inheritdoc />
        public override Task<object> DetermineValueAsync()
        {
            throw new NotImplementedException();
        }
    }
}