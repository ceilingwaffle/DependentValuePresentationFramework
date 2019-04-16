namespace DVPF.Core
{
    using System;

    /// <inheritdoc />
    /// <summary>
    /// Event data for <see cref="Node"/> state changes.
    /// </summary>
    public class NodeEventArgs : EventArgs
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:DVPF.Core.NodeEventArgs" /> class.
        /// </summary>
        /// <param name="value">
        /// The value of <see cref="T:DVPF.Core.Node" />
        /// </param>
        public NodeEventArgs(object value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Node"/>
        /// </summary>
        public object Value { get; set; }
    }
}
