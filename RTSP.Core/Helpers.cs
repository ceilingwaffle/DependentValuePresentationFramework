using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Returns true if the given class // TODO: finish description
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        internal static bool HasOverriddenProperty(Type classType, string propertyName)
        {


            // TODO: what if it's overridden with a null value...

            //var a = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            return classType.GetProperty(propertyName) != null;

            //foreach (var property in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            //{
            //    if (property.Name != propertyName)
            //    {
            //        continue;
            //    }

            //    var getMethod = property.GetGetMethod(false);
            //    if (getMethod.GetBaseDefinition() == getMethod)
            //    {
            //        return true;
            //    }
            //}

            //return false;
        }
    }

    internal enum LogCategory
    {
        Event,
        ValueChanged,

    }
}
