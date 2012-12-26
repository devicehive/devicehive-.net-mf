using System;

namespace DeviceHive
{
    /// <summary>
    /// Equipment data structure
    /// </summary>
    /// <remarks>
    /// Represents the Equipment data object compliant to HTTP API v.6 specification.
    /// This object is a part of <see cref="Device">Device</see> data structure.
    /// </remarks>
    [Serializable]
    public class Equipment
    {
        /// <summary>
        /// Equipment name
        /// </summary>
        public string name;

        /// <summary>
        /// Equipment code
        /// </summary>
        /// <remarks>
        /// Implementers should use the equipment code to recognize the equipment.
        /// Each piece of equipment within a device should have the unique code.
        /// </remarks>
        public string code;

        /// <summary>
        /// Equipment type
        /// </summary>
        /// <remarks>
        /// Name of an equipment type.
        /// Was introduced in v.6.
        /// </remarks>
        public string type;
    }
}
