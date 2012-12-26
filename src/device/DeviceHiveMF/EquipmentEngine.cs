using Microsoft.SPOT;

namespace DeviceHive
{
    /// <summary>
    /// Implementation of common equipment functionality
    /// </summary>
    /// <remarks>
    /// Implementers should derive from this class when creating their own custom equipment. See <see cref="DeviceHive.CommonEquipment.Switch"> Switch</see> class for more details.
    /// </remarks>
    public abstract class EquipmentEngine : Equipment
    {
        /// <summary>
        /// Return device that owns the equipment
        /// </summary>
        public DeviceEngine ParentDevice
        {
            get;
            private set;
        }

        /// <summary>
        /// Constricts an equipment for the specififed parent device
        /// </summary>
        /// <param name="dev">Device for which the equipment is created</param>
        public EquipmentEngine(DeviceEngine dev) : base()
        {
            ParentDevice = dev;

            //deviceClass = dev.DeviceData.deviceClass;
            //equipmentType = new EquipmentType();
        }

        /// <summary>
        /// Executes an equipment commmand
        /// </summary>
        /// <param name="cmd">Command data structure</param>
        /// <returns>True if the command succeeded; false - otherwise</returns>
        /// <remarks>
        /// Implementers should override this function with an equipment-specific command code.
        /// Common approact is to check the command code and perform actions accordingly
        /// </remarks>
        /// <example>
        /// It would be great to create an example of OnCommand usage.
        /// </example>
        public abstract bool OnCommand(DeviceCommand cmd);

        /// <summary>
        /// Registers the equipment
        /// </summary>
        /// <returns>True if the registration succeeded; false - otherwise</returns>
        /// <remarks>
        /// When the device initializes, it initializes all the equipment it has. By overriding this function an equipment can provide custom initialization steps.
        /// </remarks>
        public virtual bool Register()
        {
            return true;
        }
        
        /// <summary>
        /// Unregisters the equipment 
        /// </summary>
        /// <returns>True if unregister succeeded; false - otherwise</returns>
        /// <remarks>
        /// When the device is disconnected from the network or enters the fault state, it tries to unregister all its equipment.
        /// Implementers should override this function to provide custon uninitialization steps (closing files or handles, stopping timers, etc.).
        /// </remarks>
        public virtual bool Unregister()
        {
            return true;
        }

        /// <summary>
        /// Common functionality for sending equipment notifications
        /// </summary>
        /// <param name="DataName">Name of the equipment</param>
        /// <param name="DataValue">New value of the equipment</param>
        /// <returns>True if the notificatrion has been successfully sent; false - otherwise</returns>
        /// <remarks>Implementers can use this functuions in their custom code to notify of equipment value changes.</remarks>
        public virtual bool SendNotification(string DataName, object DataValue)
        {
            if (ParentDevice == null)
            {
                Debug.Print("unexpected parent device missing");
                return false;
            }
            else
            {
                Debug.Print("Sending notification");
                return ParentDevice.SendNotification(new EquipmentNotification(code, DataName, DataValue));
            }
        }


    }
}
