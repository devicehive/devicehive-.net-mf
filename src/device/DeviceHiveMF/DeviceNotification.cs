using System;
using System.Collections;

namespace DeviceHive
{
    /// <summary>
    /// Device notification data structure
    /// </summary>
    /// <remarks>
    /// Represents the DeviceNotification data object compliant to HTTP API v.6 specification.
    /// This object is used in /device/{ID}/notification, /device/{ID}/notification/poll, /device/{ID}/notification/poll/{ID} GET and POST commands.
    /// </remarks>
    [Serializable]
    public class DeviceNotification
    {
        /// <summary>
        /// Timestamp of a notification
        /// </summary>
        /// <remarks>
        /// Created at server.
        /// </remarks>
        public string timestamp;

        /// <summary>
        /// Name of notification
        /// </summary>
        public string notification;

        /// <summary>
        /// Name-value array of notification parameters
        /// </summary>
        public Hashtable parameters;

    }
}
