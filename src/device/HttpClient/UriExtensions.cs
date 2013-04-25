using System;
using Microsoft.SPOT;
using System.Text;

namespace DeviceHive
{
    public static class UriExtensions
    {
        static bool IsSafeUriChar(this char c)
        {
            return c == '.' || c == '-' || c == '_' || c == '~'
                || c >= 'a' && c <= 'z'
                || c >= 'A' && c <= 'Z'
                || c >= '0' && c <= '9';
        }

        public static string EscapeDataString(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            string res = "";
            foreach (byte b in bytes)
            {
                char c = (char)b;
                if (c.IsSafeUriChar())
                {
                    res += c;
                }
                else
                {
                    res += '%' + b.ToHex();
                }
            }
            return res;
        }
    }
}
