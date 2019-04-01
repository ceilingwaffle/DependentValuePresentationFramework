using System;

namespace DVPF.Core
{
    public class StatePropertyAttribute : Attribute
    {
        /// <summary>
        /// If true, this Node's value will be included on the State.
        /// </summary>
        internal bool Enabled { get; private set; } = false;
        /// <summary>
        /// The property name of this Node's value on the State.
        /// </summary>
        internal string Name { get; private set; } = null;
        /// <summary>
        /// If true, all preceder Node values must be "up to date", otherwise this Node's value will be set to null on the State.
        /// </summary>
        internal bool StrictValue { get; private set; } = false;

        public StatePropertyAttribute(bool enabled, string name, bool strictValue = false)
        {
            Enabled = enabled;
            Name = name;
            StrictValue = strictValue;
        }

    }
}
