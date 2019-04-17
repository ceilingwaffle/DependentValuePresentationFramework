namespace DVPF.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <inheritdoc />
    /// <summary>
    /// Optionally applied as an attribute on a derived <seealso cref="Node" /> class to present that node's value as a property of the <seealso cref="State" />.
    /// </summary>
    public class StatePropertyAttribute : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="StatePropertyAttribute" /> class.
        /// </summary>
        /// <param name="enabled">
        /// See <seealso cref="P:DVPF.Core.StatePropertyAttribute.Enabled" />
        /// </param>
        /// <param name="name">
        /// See <seealso cref="P:DVPF.Core.StatePropertyAttribute.Name" />
        /// </param>
        /// <param name="strictValue">
        /// See <seealso cref="P:DVPF.Core.StatePropertyAttribute.StrictValue" />
        /// </param>
        public StatePropertyAttribute(bool enabled, string name, bool strictValue = false)
        {
            this.Enabled = enabled;
            this.Name = name;
            this.StrictValue = strictValue;
        }

        /// <summary>
        /// Gets a value indicating whether the value of the <seealso cref="Node"/> is to be included as a property of the <see cref="State"/>.
        /// </summary>
        internal bool Enabled { get; }

        /// <summary>
        /// Gets the name of the property to be included as the key name of <seealso cref="State.Properties"/>.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// <para>Gets a value indicating whether the value of the target <see cref="Node"/> should be set to null as soon as any parent value changes.</para>
        /// <para>This is used to prevent a follower-node's value existing whose value is based on old values from preceder-nodes.</para>
        /// <para>See also: <seealso cref="Node.NullifyValueWithoutShiftingToPrevious"/></para>
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        internal bool StrictValue { get; }
    }
}
