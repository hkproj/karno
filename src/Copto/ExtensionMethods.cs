using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Copto
{
    public static class ExtensionMethods
    {

        /// <summary>
        /// Returns true if <paramref name="str"/> starts with any of the prefixes in <paramref name="prefixes"/>
        /// </summary>
        public static bool StartsWithAny(this string str, params char[] prefixes)
        {
            if (str.Length == 0) return false; // Empty string
            var start = str[0];

            foreach(var prefix in prefixes)
                if (start == prefix)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns true if <paramref name="str"/> ends with any of the suffixes in <paramref name="suffixes"/>
        /// </summary>
        public static bool EndsWithAny(this string str, params char[] suffixes)
        {
            if (str.Length == 0) return false; // Empty string
            var end = str[str.Length - 1];

            foreach (var suffix in suffixes)
                if (end == suffix)
                    return true;

            return false;
        }

    }
}
