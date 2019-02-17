using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTSP.Core
{
    internal static class Helpers
    {
        static Random rnd = new Random();

        internal static int UnixTimestamp()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        internal static int Rand(int from, int to)
        {
            return rnd.Next(from, to + 1);
        }
    }

    internal enum LogCategory
    {
        Event,
        ValueChanged,

    }
}
