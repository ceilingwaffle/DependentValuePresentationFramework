using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DVPF.Core
{
    public class NodeEventArgs : EventArgs
    {
        public object Value { get; set; }

        public NodeEventArgs(object value)
        {
            Value = value;
        }
    }
}
