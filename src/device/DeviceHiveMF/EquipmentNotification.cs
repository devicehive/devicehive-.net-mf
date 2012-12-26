using System.Collections;

namespace DeviceHive
{
    /// <summary>
    /// Equipment notification class
    /// </summary>
    /// <remarks>
    /// This is a helper class for equipment notification sending.
    /// It should be used by implementers to effectively send equipment notifications.
    /// </remarks>
    public class EquipmentNotification : INotification
    {
        private const string CommandName = "equipment";

        /// <summary>
        /// Constructs an equipment notification by given parameters
        /// </summary>
        /// <param name="EquipmentCode">Equipment code</param>
        /// <param name="DataName">Parameter name</param>
        /// <param name="ParameterValue">Parameter value</param>
        /// <remarks>
        /// Implementers should create instances of this class to pass to <see cref="DeviceEngine.SendNotification">DeviceEngine.SendNotification</see> function.
        /// </remarks>
        public EquipmentNotification(string EquipmentCode, string DataName, object ParameterValue)
        {
            Data = new DeviceNotification()
            {
                notification = CommandName,
                timestamp = null,
                parameters = new Hashtable()
            };
            Data.parameters.Add(CommandName, EquipmentCode);
            Data.parameters.Add(DataName, ParameterValue.ToString());
        }

        /// <summary>
        /// Equipment notifdication data
        /// </summary>
        /// <remarks>
        /// Usually, implementers would not need to access this property - it is initialized by constructor.
        /// </remarks>
        public DeviceNotification Data
        {
            get;
            private set;
        }
    }
}
