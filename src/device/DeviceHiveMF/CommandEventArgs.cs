using Microsoft.SPOT;

namespace DeviceHive
{
    /// <summary>
    /// Delegate for a command that does not return a result.
    /// </summary>
    /// <param name="sender">Object that sends an event</param>
    /// <param name="e"><see cref="DeviceHive.CommandEventArgs">CommandEventArgs</see> event parameters </param>
    /// <remarks>
    /// Use this delegate in your device to handle <see cref="DeviceHive.DeviceEngine.BeforeCommand">BeforeCommand</see> and <see cref="DeviceHive.DeviceEngine.AfterCommand">AfterCommand</see> events.
    /// </remarks>
    /// <example > This example shows ho to add command handlers to your device.
    /// <code>
    /// public class SampleDevice : DeviceEngine 
    /// {
    ///     public SampleDevice() 
    ///     {   
    ///         // Initialising members here 
    ///         // ... 
    ///         BeforeCommand += new CommandEventHandler(OnBeforeCommand); 
    ///         AfterCommand += new CommandEventHandler(OnAfterCommand); 
    ///         // ... 
    ///     } 
    ///     
    ///     private void OnBeforeCommand(object sender, CommandEventArgs e)
    ///     {
    ///         Debug.Print(e.Command.id);
    ///     }
    ///     
    ///     private void OnAfterCommand(object sender, CommandEventArgs e)
    ///     {
    ///         Debug.Print(e.Command.result);
    ///     }
    /// }
    /// </code>
    /// </example>
    public delegate void CommandEventHandler(object sender, CommandEventArgs e);

    /// <summary>
    /// Delegate for a command that returns a result.
    /// </summary>
    /// <param name="sender">Object that sends an event</param>
    /// <param name="e"><see cref="DeviceHive.CommandEventArgs">CommandEventArgs</see> event parameters </param>
    /// <returns>True if command had been executed successfully; flase - otherwise</returns>
    /// <remarks>
    /// Use this delegate in your device to handle <see cref="DeviceHive.DeviceEngine.DeviceCommand">DeviceCommand</see> event.
    /// </remarks>
    /// <example > This example shows ho to add device command handler to your device.
    /// <code>
    /// public class SampleDevice : DeviceEngine 
    /// {
    ///     public SampleDevice() 
    ///     {   
    ///         // Initialising members here 
    ///         // ... 
    ///         DeviceCommand += new CommandResultEventHandler(OnDeviceCommand);
    ///         // ... 
    ///     } 
    ///     
    ///     bool ThemoCloudDevice_DeviceCommand(object sender, CommandEventArgs e)
    ///     {
    ///         // Check e.Command
    ///         // and perform custom actions
    ///         // ...
    ///         return true; // Processing complete; command was handled
    ///     }
    /// }
    /// </code>
    /// </example>
    public delegate bool CommandResultEventHandler(object sender, CommandEventArgs e);

    /// <summary>
    /// Event arguments for a DeviceHive command
    /// </summary>
    public class CommandEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a command event arguments structure
        /// </summary>
        /// <param name="cmd">Command that had caused an event. Should be expected to be null.</param>
        public CommandEventArgs(DeviceCommand cmd)
        {
            Command = cmd;
        }

        /// <summary>
        /// Command that had caused the event
        /// </summary>
        /// <remarks>This command would be available in the handler function</remarks>
        /// <example> This example demonstrates how to use the Command property.
        /// <code>
        /// private void OnBeforeCommand(object sender, CommandEventArgs e)
        /// {
        ///     Debug.Print(e.Command.id);
        /// }
        /// </code>
        /// </example>
        public DeviceCommand Command;
    }
}
