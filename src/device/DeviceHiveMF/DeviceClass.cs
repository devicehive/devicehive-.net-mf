using System;
using System.Collections;

namespace DeviceHive
{
    /// <summary>
    /// Device class structure.
    /// </summary>
    /// <remarks>
    /// Represents the DeviceClass data object compliant to HTTP API v.6 specification.
    /// This object is a part of <see cref="Device">Device</see> data structure.
    /// </remarks>
    [Serializable]
    public class DeviceClass
    {
        /// <summary>
        /// Class name
        /// </summary>
        public string name;

        /// <summary>
        /// Class version
        /// </summary>
        /// <remarks>
        /// For example, 1.0.0.4
        /// </remarks>
        public string version;

        /// <summary>
        /// Amount of inactivity time, in milliseconds, after which the server would consider that the device offline.
        /// </summary>
        public int offlineTimeout;

        /// <summary>
        /// Associated device class data.
        /// </summary>
        public Hashtable data;

        public override int GetHashCode()
        {
            return ObjectHelpers.GetHashCode(name) 
                ^ ObjectHelpers.GetHashCode(version) 
                ^ ObjectHelpers.GetHashCode(offlineTimeout) 
                ^ ObjectHelpers.GetHashCode(data);
        }
    }
}
