using System;
using Microsoft.SPOT;

namespace DeviceHive
{
    static class ObjectHelpers
    {
        public static int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return obj.GetHashCode();
        }
    }
}
