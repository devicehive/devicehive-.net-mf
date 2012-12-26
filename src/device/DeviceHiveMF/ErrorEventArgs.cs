using System;
using Microsoft.SPOT;

namespace DeviceHive
{
    /// <summary>
    /// Error handler delegate function
    /// </summary>
    /// <param name="sender">Object that send an error event</param>
    /// <param name="e">Error parameters, <see cref="DeviceHive.ErrorEventArgs">ErrorEventArgs</see> for more information</param>
    /// <returns>True if the device should reconnect after an error anf false - if it can proceed without disconnection</returns>
    /// <remarks>
    /// This delegate should be used by implementers to handle <see cref="DeviceEngine.Error">DeviceEngine.Error</see> event.
    /// </remarks>
    public delegate bool ErrorEventHandler(object sender, ErrorEventArgs e);

    /// <summary>
    /// Error event arguments structure
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Exception that caused an error
        /// </summary>
        /// <remarks>
        /// Can be null.
        /// </remarks>
        public Exception ex;

        /// <summary>
        /// Constructs error event arguments structure for a specified exception
        /// </summary>
        /// <param name="exception">Exception which caused an error</param>
        public ErrorEventArgs(Exception exception)
        {
            ex = exception;
        }
    }
}
