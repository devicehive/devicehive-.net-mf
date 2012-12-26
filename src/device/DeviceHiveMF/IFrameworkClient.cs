
namespace DeviceHive
{
    /// <summary>
    /// Framework client interface
    /// </summary>
    /// <remarks>
    /// This interface represents a transport layer. It declares commmunication logic. It is used within the Framework classes.
    /// This interface can be implemented to support different data transport protocols.
    /// </remarks>
    public interface IFrameworkClient
    {
        /// <summary>
        /// Registers a device
        /// </summary>
        /// <param name="device">Device data structure</param>
        /// <returns>True, if registration succeeds; false - otherwise</returns>
        /// <remarks>
        /// This method is called by device to register it at the server.
        /// </remarks>
        bool SetDevice(Device device);

        /// <summary>
        /// Sends a notification
        /// </summary>
        /// <param name="device">Device data of the device that is sending the notification</param>
        /// <param name="notification">Notification to be sent</param>
        /// <returns>True if the notification succeeds; false - otherwise</returns>
        /// <remarks>
        /// This method can be used by devices to send notifications.
        /// </remarks>
        bool PostNotification(Device device, INotification notification);

        /// <summary>
        /// Gets a command
        /// </summary>
        /// <param name="device">Device data of the device that is getting a command</param>
        /// <returns>Command data structure or null if there are no pending commands</returns>
        /// <remarks>
        /// This method returns all the commands that are waiting at the server since last GetCommand call. It returns immediately, regardless if there are commands to execute or not.
        /// </remarks>
        DeviceCommand GetCommand(Device device);

        /// <summary>
        /// Polls command for execution
        /// </summary>
        /// <param name="device">Device data of the device that is polling a command</param>
        /// <returns>Command data structure or null if there are no commands</returns>
        /// <remarks>
        /// This method returns the next command from the command queue. If there are no commands in the queue, it waits for ~30 seconds to receive a new command.
        /// </remarks>
        DeviceCommand PollCommand(Device device);

        /// <summary>
        /// Updates command status
        /// </summary>
        /// <param name="device">Device data of the device that is updating a command status</param>
        /// <param name="CommandId">ID of the command to be updated</param>
        /// <param name="status">Status of the command</param>
        /// <returns>True if the status update was successfull; false - otherwise</returns>
        /// <remarks>
        /// The devices are using this method to notify the server of command completion and its result. 
        /// </remarks>
        bool UpdateCommandStatus(Device device, string CommandId, CommandStatus status);
    }
}
