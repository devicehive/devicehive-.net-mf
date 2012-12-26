using System;

namespace DeviceHive
{
    /// <summary>
    /// Device network data structure
    /// </summary>
    /// <remarks>
    /// Represents the Network data object compliant to HTTP API v.6 specification.
    /// Used as a network field in the device initialization structure for call to /device/{ID} GET and PUT
    /// </remarks>
    [Serializable]
    public class DeviceNetwork
    {
        /// <summary>
        /// Network name
        /// </summary>
        public string name;

        /// <summary>
        /// Network description
        /// </summary>
        /// <remarks>
        /// Free text
        /// </remarks>
        public string description;

        /// <summary>
        /// Network security key
        /// </summary>
        /// <remarks>
        /// Introduced in v.6.
        /// It is recommended to use string GUID when creating this data structure.
        /// </remarks>
        public string key; // v6
    }
}
