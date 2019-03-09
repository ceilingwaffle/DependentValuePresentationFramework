using System;

namespace RTSP.Core
{
    public class StatePropertyAttribute : Attribute
    {
        internal bool Enabled { get; private set; } = false;
        internal string Name { get; private set; } = null;

        public StatePropertyAttribute(bool enabled, string name)
        {
            Enabled = enabled;
            Name = name;
        }

    }
}
