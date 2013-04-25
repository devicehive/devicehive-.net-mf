using System;
using Microsoft.SPOT;

namespace DeviceHive
{
    /// <summary>
    /// Represents meta-information about the current API.
    /// </summary>
    [Serializable]
    public class ApiInfo
    {
        /// <summary>
        /// API version.
        /// </summary>
        public string apiVersion;

        /// <summary>
        /// Current server timestamp.
        /// </summary>
        public string serverTimestamp;

        /// <summary>
        /// WebSocket server URL
        /// </summary>
        public string webSocketServerUrl;

        public override int GetHashCode()
        {
            return ObjectHelpers.GetHashCode(apiVersion)
                ^ ObjectHelpers.GetHashCode(serverTimestamp)
                ^ ObjectHelpers.GetHashCode(webSocketServerUrl);
        }
    }
}
