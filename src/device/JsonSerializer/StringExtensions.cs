using System;
using Microsoft.SPOT;
using System.Text;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Extension attribute
    /// </summary>
    /// <remarks>
    /// A workaroung needed to make it build.
    /// </remarks>
    public class ExtensionAttribute : Attribute { }
} 

namespace Json.Serialization
{
    /// <summary>
    /// Extension methods for string manipulations
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string to GUID
        /// </summary>
        /// <param name="s">string GUID</param>
        /// <returns>Guid from the given string</returns>
        public static Guid ToGuid(this string s)
        {
            string[] parts = s.Split('-');
            string fs = string.Concat(parts);
            int n = fs.Length / 2;

            byte[] bts = new byte[n];
            for (int x = 0; x < n; ++x)
            {
                bts[x] = byte.Parse(fs.Substring(x, 2));
            }
            Guid uid = new Guid(bts);
            return uid;
        }

        /// <summary>
        /// Converts a string to float
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <returns>floating point number</returns>
        public static float ToFloat(this string s)
        {
            double d = double.Parse(s);
            float f = (float)d;
            return f;
        }

        /// <summary>
        /// COnverts a string to DateTime using DeviceHive notation
        /// </summary>
        /// <param name="s">string to convert</param>
        /// <returns>DateTime object</returns>
        public static DateTime ToDateTime(this string s)
        {
            string[] parts = s.Split('T', '-', ':', '.');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);
            int hour = int.Parse(parts[3]);
            int min = int.Parse(parts[4]);
            int sec = int.Parse(parts[5]);
            int sec100 = int.Parse(parts[6]);

            DateTime dt = new DateTime(year, month, day, hour, min, sec, sec100 / 1000);
            dt += new TimeSpan(sec100 % 1000 * 10);

            return dt;
        }

        /// <summary>
        /// Replaces the set of characters in a string with given substring
        /// </summary>
        /// <param name="s">string to be searched for sequence</param>
        /// <param name="Target">substring to be replaced</param>
        /// <param name="ReplaceWith">new text</param>
        /// <returns>modified string</returns>
        public static string Replace(this string s, string Target, string ReplaceWith)
        {
            string rv = string.Empty;
            int n = s.IndexOf(Target);
            int oldN = 0;
            while (n != -1)
            {
                rv += s.Substring(oldN, n - oldN) + ReplaceWith;
                n += Target.Length;
                oldN = n;
                n = s.IndexOf(Target, n);
            }
            rv += s.Substring(oldN, s.Length - oldN);
            return rv;
        }

        /// <summary>
        /// Checks if the string is null or empty
        /// </summary>
        /// <param name="s">string to be checked</param>
        /// <returns>True if the string = null of is empty (""); false - otherwise</returns>
        public static bool IsNullOrEmpty(string s)
        {
            if (s == null) return true;
            return s == string.Empty;
        }
    }
}
