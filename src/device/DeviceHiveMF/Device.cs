using System;
using System.Collections;

namespace DeviceHive
{
    /// <summary>
    /// Device data structure.
    /// 
    /// </summary>
    /// <remarks>
    /// Represents the Device data object compliant to HTTP API v.6 specification.
    /// This object is used for providing device information for /device/{ID} PUT message.
    /// </remarks>
    [Serializable]
    public class Device
    {
        /// <summary>
        /// Gets/sets device UID
        /// </summary>
        /// <remarks>
        /// Implementor should create this ID and use it when registering the device.
        /// </remarks>
        public Guid id
        {
            get;
            set;
        }
        
        /// <summary>
        /// Device security key. It is usually a string containing a GUID.
        /// </summary>
        /// <remarks>
        /// Implementer should create this security key and use it in all messages related to the device. Usually the key is a GUID string.
        /// </remarks>
        public string key; // v6

        /// <summary>
        /// Device name
        /// </summary>
        public string name;

        /// <summary>
        /// Device status
        /// </summary>
        public string status;

        /// <summary>
        /// Associated device data.
        /// </summary>
        public Hashtable data;

        /// <summary>
        /// Device network
        /// </summary>
        public DeviceNetwork network;

        /// <summary>
        /// Device class
        /// </summary>
        public DeviceClass deviceClass;

        /// <summary>
        /// Array of device equipment structures
        /// </summary>
        public Equipment[] equipment;

        public override int GetHashCode()
        {
            return ObjectHelpers.GetHashCode(id)
                ^ ObjectHelpers.GetHashCode(key)
                ^ ObjectHelpers.GetHashCode(name)
                ^ ObjectHelpers.GetHashCode(status)
                ^ ObjectHelpers.GetHashCode(data)
                ^ ObjectHelpers.GetHashCode(network)
                ^ ObjectHelpers.GetHashCode(deviceClass)
                ^ ObjectHelpers.GetHashCode(equipment);
        }
    }
}
