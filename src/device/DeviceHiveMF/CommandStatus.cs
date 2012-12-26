using System;
using Microsoft.SPOT;

namespace DeviceHive
{   
    /// <summary>
    /// Command status structure.
    /// Represents a data object compliant to HTTP API v.6 specification.
    /// This object is returned in /devices/{ID}/command/{ID} PUT message.
    /// </summary>
    [Serializable]
    public class CommandStatus
    {
        /// <summary>
        /// Command status constant. Represents a successful completion.
        /// </summary>
        /// <value>Completed</value>
        public const string Completed = "Completed";

        /// <summary>
        /// Command status constant. Represents a command failure.
        /// </summary>
        /// <value>Failed</value>
        public const string Failed = "Failed";

        /// <summary>
        /// Command status string. Should be equal to either Completed or Failed constant.
        /// </summary>
        public string status;

        /// <summary>
        /// Command result string. It is a free text message.
        /// </summary>
        public string result;
    }
}
