using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace RTSP.Core
{
    public class State
    {
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public State()
        {

        }

        public object this[string propertyName]
        {
            get
            {
                return Properties[propertyName];
            }
            set
            {
                Properties[propertyName] = value;
            }

        }

        public override string ToString()
        {
            return string.Join(
                System.Environment.NewLine,
                Properties.Select(p => $"{p.Key, 15}: {p.Value.ToString()}")
            );
        }
    }
}
