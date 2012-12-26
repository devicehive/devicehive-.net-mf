
namespace DeviceHive
{
    /// <summary>
    /// Device status notification
    /// </summary>
    /// <remarks>
    /// This is a helper class that allows fast notification creation. It can be used to create and initialize device-related notifications.
    /// </remarks>
    public class DeviceStatusNotification : INotification
    {
        
        private const string CommandName = "DeviceStatus";
        private const string StatusParameter = "status";

        /// <summary>
        /// Creates a device notification by a specified specified message string
        /// </summary>
        /// <param name="status">Message string for notification</param>
        /// <remarks>
        /// This class can be used by implementers to effectively construct device notifications.
        /// Implementers should create instances of this class to pass to <see cref="DeviceEngine.SendNotification">DeviceEngine.SendNotification</see> function.
        /// </remarks>
        /// <example>
        /// The following example shows how to send a device status notification.
        /// <code>
        /// public class SampleDevice : DeviceEngine
        /// {
        ///     public SampleDevice()
        ///     {
        ///         // Perfdorm initialization here
        ///         // ... 
        ///         DeviceCommand += new CommandResultEventHandler(OnDeviceCommand);
        ///     }
        ///     
        ///     // ... 
        ///     
        ///     bool ThemoCloudDevice_DeviceCommand(object sender, CommandEventArgs e)
        ///     {
        ///         DeviceStatusNotification dn = new DeviceStatusNotification("test status notification");
        ///         return SendNotification(dn);
        ///     }
        /// }
        /// </code>
        /// </example>
        public DeviceStatusNotification(string status)
        {
            Data = new DeviceNotification()
            {
                notification = CommandName,
                timestamp = null,
                parameters = new System.Collections.Hashtable()
            };
            Data.parameters.Add(StatusParameter, status);

        }

        /// <summary>
        /// Device notification object that holds notification data
        /// </summary>
        /// <remarks>
        /// Normally, implementers do not have to use this property directly. It is initialized through the constructor.
        /// </remarks>
        public DeviceNotification Data
        {
            get;
            private set;
        }

    }
}
