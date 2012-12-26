using Microsoft.SPOT;

namespace DeviceHive
{
    /// <summary>
    /// Connection event handler class
    /// </summary>
    /// <param name="sender">Object that has sent an event. It is expected to be a DeviceEngine descendant.</param>
    /// <param name="e">Standard event arguments. Carry no information.</param>
    /// <returns>True if connection succeeded; false - otherwise</returns>
    /// <remarks>
    /// Use this handler to handle Connecting and Connected events
    /// </remarks>
    /// <example>
    /// This example shows how to add handlers to connection events
    /// <code>
    
    /// public class SampleDevice : DeviceEngine
    /// {
    ///     public SampleDevice()
    ///     {
    ///         // Initializing members here
    ///         // ...
    ///         Connecting += new ConnectEventHandler(OnConnecting);
    ///         Connected += new ConnectEventHandler(OnConnected);
    ///     }
    ///     
    ///     bool OnConnecting(object sender, EventArgs e)
    ///     {
    ///         // ...
    ///         return true;
    ///     }
    ///     
    ///     bool OnConnected(object sender, EventArgs e)
    ///     {
    ///         // ...
    ///         return true;
    ///     }
    /// }
    /// </code>
    /// </example>
    public delegate bool ConnectEventHandler(object sender, EventArgs e);
}
