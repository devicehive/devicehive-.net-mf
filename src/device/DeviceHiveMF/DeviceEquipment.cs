using System;
using System.Collections;

namespace DeviceHive
{
    /// <summary>
    /// Equipment data structure
    /// </summary>
    /// <remarks>
    /// Represents the DeviceEquipment data object compliant to HTTP API v.6 specification.
    /// Used to specify command parameters in /device/{ID}/command GET, POST and PUT messages.
    /// </remarks>
    [Serializable]
    public class DeviceEquipment
    {
        /// <summary>
        /// Timestamp of the command
        /// </summary>
        /// <remarks>
        /// Created by server.
        /// </remarks>
        public DateTime timestamp;

        /// <summary>
        /// Command name
        /// </summary>
        public string name;

        /// <summary>
        /// Name-value array of parameters
        /// </summary>
        public Hashtable parameters;
    }
}
