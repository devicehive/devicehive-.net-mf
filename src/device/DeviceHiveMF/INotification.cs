
namespace DeviceHive
{
    /// <summary>
    /// Device notification interface
    /// </summary>
    /// <remarks>
    /// Is a bace for specific device and equipment notifications.
    /// Implementers should inherit from it to define specific notifications.
    /// </remarks>
    public interface INotification
    {
        /// <summary>
        /// Returns the notification data structure
        /// </summary>
        /// <remarks>
        /// The notification data structure is usually initialized in constructors of derived notification classes.
        /// </remarks>
        DeviceNotification Data
        {
            get;
        }
    }
}
