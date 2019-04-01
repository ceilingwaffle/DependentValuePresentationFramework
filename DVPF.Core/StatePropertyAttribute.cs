using System;

namespace DVPF.Core
{
    public class StatePropertyAttribute : Attribute
    {
        internal bool Enabled { get; private set; } = false;
        internal string Name { get; private set; } = null;
        internal bool StrictValue { get; private set; } = false;

        /// <summary>
        /// Determines how the value of a Node is presented to the State.
        /// </summary>
        /// <param name="enabled">If true, this Node's value will be included on the State.</param>
        /// <param name="name">The property name of this Node's value on the State.</param>
        /// <param name="strictValue">If true, all preceder Node values must be "up to date", otherwise this Node's value will be set to null on the State.</param>
        public StatePropertyAttribute(bool enabled, string name, bool strictValue = false)
        {
            Enabled = enabled;
            Name = name;
            StrictValue = strictValue;
        }

    }
}
