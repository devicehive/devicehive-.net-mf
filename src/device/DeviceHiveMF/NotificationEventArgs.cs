using System;
using Microsoft.SPOT;

namespace DeviceHive
{
    /// <summary>
    /// Delegate function of the notification event 
    /// </summary>
    /// <param name="sender">Object that has sent a notification</param>
    /// <param name="e">Notification parameters</param>
    /// <remarks>Implementers should use this delegate to handle <see cref="DeviceEngine.Error">DeviceEngine.Error</see> events</remarks>
    public delegate void NotificationEventHandler(object sender, NotificationEventArgs e);

    /// <summary>
    /// Notification event arguments structure
    /// </summary>
    public class NotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs notification event arguments structure for the specified notification
        /// </summary>
        /// <param name="n">Notification data</param>
        /// <remarks>
        /// Implementers should expect that Notification parameter can be null.
        /// </remarks>
        public NotificationEventArgs(INotification n)
        {
            Notification = n;
        }

        /// <summary>
        /// Notification data
        /// </summary>
        public INotification Notification;
    }
}
