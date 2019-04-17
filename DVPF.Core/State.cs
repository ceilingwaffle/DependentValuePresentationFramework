namespace DVPF.Core
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The state to contain properties of the most recent values of each enabled <seealso cref="Node"/> ("enabled" is defined by nodes with attribute <seealso cref="StatePropertyAttribute.Enabled"/> set to true).
    /// </summary>
    public class State
    {
        /// <summary>
        /// Gets a collection of values for each "enabled" <seealso cref="Node"/> (defined by the <seealso cref="StatePropertyAttribute"/> attribute per node).
        /// </summary>
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Property name (key) (defined by <seealso cref="StatePropertyAttribute"/>) with the corresponding <seealso cref="Node"/> value (value).
        /// </summary>
        /// <param name="propertyName">
        /// The property name defined by <seealso cref="StatePropertyAttribute.Name"/> applied to a specific <seealso cref="Node"/>
        /// </param>
        /// <returns>
        /// The value of the corresponding <seealso cref="Node"/>
        /// </returns>
        public object this[string propertyName]
        {
            get => this.Properties[propertyName];
            set => this.Properties[propertyName] = value;
        }

        /// <summary>
        /// Returns all the state property names and values together as a formatted string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Join(
                System.Environment.NewLine,
                this.Properties.Select(p => $"{p.Key,15}: {p.Value?.ToString()}"));
        }
    }
}
