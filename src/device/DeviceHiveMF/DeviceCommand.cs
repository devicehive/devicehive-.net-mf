using System;
using System.Collections;

namespace DeviceHive
{
    /// <summary>
    /// Device command structure.
    /// </summary>
    /// <remarks>
    /// Represents the DeviceCommand data object compliant to HTTP API v.6 specification.
    /// This object is received in /device/{ID}/command GET/POST messaages.
    /// </remarks>
    [Serializable]
    public class DeviceCommand
    {
        /// <summary>
        /// Command ID
        /// </summary>
        /// <remarks>
        /// Created and returned by server.
        /// </remarks>
        public string id;

        /// <summary>
        /// Timestamp of a command.
        /// </summary>
        /// <remarks>
        /// Caller can specify the timestamp when getting or polling commands.
        /// </remarks>
        public string timestamp;

        /// <summary>
        /// UserId of creator of the command.
        /// </summary>
        public int userId;

        /// <summary>
        /// Command name
        /// </summary>
        public string command;

        /// <summary>
        /// Table of command parameters
        /// </summary>
        public Hashtable parameters;

        /// <summary>
        /// Command life time
        /// </summary>
        public int lifetime;

        /// <summary>
        /// Command flags
        /// </summary>
        public int flags;

        /// <summary>
        /// Command status
        /// </summary>
        public string status;

        /// <summary>
        /// Command result
        /// </summary>
        public string result;

        public override int GetHashCode()
        {
            return ObjectHelpers.GetHashCode(id)
                ^ ObjectHelpers.GetHashCode(timestamp)
                ^ ObjectHelpers.GetHashCode(userId)
                ^ ObjectHelpers.GetHashCode(command)
                ^ ObjectHelpers.GetHashCode(parameters)
                ^ ObjectHelpers.GetHashCode(lifetime)
                ^ ObjectHelpers.GetHashCode(flags)
                ^ ObjectHelpers.GetHashCode(status)
                ^ ObjectHelpers.GetHashCode(result);
        }
    }
}
