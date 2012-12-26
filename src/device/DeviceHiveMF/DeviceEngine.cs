using System;
using Microsoft.SPOT;

namespace DeviceHive
{
    /// <summary>
    /// Base class for DeviceHive .NET Micro Framework devices.
    /// All custom devices should be inherited from it.
    /// </summary>
    public abstract class DeviceEngine
    {
        private const string EquipmentParameter = "equipment";
        private const int DefaultReconnectCount = 5;

        /// <summary>
        /// DeviceHive client used to handle API calls.
        /// </summary>
        protected IFrameworkClient DcClient;

        /// <summary>
        /// Returns device's connection status. True means that the device is online and has successfully regiistered itself in DeviceHive network.
        /// </summary>
        public bool IsConnected
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns device's online status. Is true iif the network connection is present.
        /// </summary>
        public bool IsOnline
        {
            get;
            private set;
        }

        /// <summary>
        /// Number of reconnection attempts
        /// </summary>
        public int ReconnectCount
        {
            get;
            set;
        }
        
        /// <summary>
        /// Device data structure
        /// </summary>
        /// <remarks>
        /// Implementers should fill it when the device is constructed.
        /// </remarks>
        public Device DeviceData
        {
            get;
            private set;
        }

        /// <summary>
        /// Simple event handler delegate
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Empty event arguments</param>
        public delegate void SimpleEventHandler(object sender, EventArgs e);

        /// <summary>
        /// This event is fired before the device starts initializing the equipment
        /// </summary>
        public event ConnectEventHandler Initializing;

        /// <summary>
        /// This event is fired when the initialization is complete
        /// </summary>
        public event ConnectEventHandler Initialized;

        /// <summary>
        /// This event is fired before the device starts connecting to network
        /// </summary>
        public event ConnectEventHandler Connecting;

        /// <summary>
        /// This event is fired after the device had connected to network
        /// </summary>
        public event ConnectEventHandler Connected;

        /// <summary>
        /// This event is fired when the device has discovered that a network connection is lost
        /// </summary>
        public event SimpleEventHandler Disconnected;

        /// <summary>
        /// This event is fired before the device has started to query the server for commands
        /// </summary>
        public event CommandEventHandler BeforeCommand;

        /// <summary>
        /// This event is fired if the command received from the server hasn't been processed by any of its equipment
        /// </summary>
        public event CommandResultEventHandler DeviceCommand;

        /// <summary>
        /// This event is fired after the command has been processed
        /// </summary>
        public event CommandEventHandler AfterCommand;

        /// <summary>
        /// This event is fired before a device is going to send a notification
        /// </summary>
        public event NotificationEventHandler BeforeNotification;

        /// <summary>
        /// This event is fired after the notification had been sent
        /// </summary>
        public event NotificationEventHandler AfterNotification;

        /// <summary>
        /// This event is fired when the device has encountered an error
        /// </summary>
        public event ErrorEventHandler Error;
        
        /// <summary>
        /// This function should be overridden by implementers to return a specific <see cref="DeviceData"/> structure.
        /// </summary>
        /// <returns>DeviceData structure filled with device parameters</returns>
        protected abstract Device CreateDeviceData();

        /// <summary>
        /// Initializes device equipment structure
        /// </summary>
        /// <remarks>
        /// This function should be overridden by implementers to construct device equipment and fill DeviceData equipment hashtable with equipment.
        /// This function is invoked after the device has connected to data network and registered itself.
        /// Normally, the constructed equipment should be initialized and send its status to DeviceHive network.
        /// </remarks>
        protected abstract void CreateEquipment();

        /// <summary>
        /// Initializes the device
        /// </summary>
        /// <returns>True if the initialization was successfull</returns> 
        /// <remarks>
        /// This function should be called by implementers before any calls to other DeveiceEngine's functions.
        /// </remarks>
        virtual public bool Init()
        {
            IsConnected = false;
            IsOnline = false;
            ReconnectCount = DefaultReconnectCount;
            //DcClient = client;
            if (Initializing != null)
            {
                if (!Initializing(this, new EventArgs())) return false;
            }
            //PreInit();
            DeviceData = CreateDeviceData();
            CreateEquipment();
            //PostInit();
            if (Initialized != null)
            {
                if (!Initialized(this, new EventArgs())) return false;
            }
            return true;
        }
        
        /// <summary>
        /// Registers all equipment in DeviceHive network
        /// </summary>
        /// <returns>True if all the equipment was registered successfull</returns>
        private bool RegisterEquipment()
        {
            Debug.Print("Registering equipment.");
            //DeviceData.equipment = new Equipment[Equipment.Count];
            foreach (EquipmentEngine ei in DeviceData.equipment)
            {
                if (!ei.Register()) return false;
                //DeviceData.equipment[x] = ei.EquipmentData;
            }
            Debug.Print("Registering equipment done.");
            return true;
        }

        /// <summary>
        /// Unregisters equipment
        /// </summary>
        /// <returns>Returns true if successfull</returns>
        private bool UnregisterEquipment()
        {
            Debug.Print("Unregistering equipment.");
            try
            {
                //DeviceData.equipment = new Equipment[Equipment.Count];
                foreach (EquipmentEngine ei in DeviceData.equipment)
                {
                    if (!ei.Unregister()) return false;
                    //DeviceData.equipment[x] = ei.EquipmentData;
                }
                Debug.Print("Unregistering equipment done.");
            }
            catch (Exception ex)
            {
                Debug.Print("Error while unregistering equipment!");
                Debug.Print(ex.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Connects device to network
        /// </summary>
        /// <remarks>
        /// This function should be called by implementers to make the device connect to DeviceHive network.
        /// </remarks>
        /// <returns>True if successfull</returns>
        virtual public bool Connect()
        {
            //bool IsConnected = false;
            try
            {
                if (!IsOnline)
                {
                    if (Connecting != null)
                    {
                        if (!Connecting(this, new EventArgs())) return false;
                    }
                    IsOnline = true;
                }
                if (!IsConnected)
                {
                    if (!DcClient.SetDevice(DeviceData)) return false;
                    if (!DcClient.PostNotification(DeviceData, new DeviceStatusNotification(DeviceStatus.Online))) return false;
                    if (!RegisterEquipment()) return false;
                    IsConnected = true;
                    if (Connected != null)
                    {
                        if (!Connected(this, new EventArgs())) return false;
                    }
                }
                return true;

            }
            catch (Exception ex)
                // System.Net.WebException
            {
                Debug.Print(ex.ToString());
                
                bool doDisconnect = false;
                if (Error != null)
                {
                    doDisconnect = Error(this, new ErrorEventArgs(ex));
                }
                
                if (doDisconnect)
                {
                    IsConnected = false;
                    IsOnline = false;
                    
                }
                return false;
            }
        }

        /// <summary>
        /// Locates an equipment by a specified code
        /// </summary>
        /// <param name="code">Equipment code</param>
        /// <returns>Equipment object</returns>
        private EquipmentEngine FindDevice(string code)
        {
            EquipmentEngine rv = null;

            //Equipment eq = null;
            foreach (Equipment e in DeviceData.equipment)
            {
                if (e.code == code)
                {
                    rv = e as EquipmentEngine;
                    break;
                }
            }
            return rv;
        }

        /// <summary>
        /// Default command processing
        /// </summary>
        /// <returns>True if the commant processing was successfull</returns>
        /// <remarks>
        /// This function performs a default command processing, including handling equipment commands and firing events.
        /// It should be invoked by implementers to perform a single command processing loop. So, it should be ran continuously.
        /// </remarks>
        virtual public bool ProcessCommands()
        {
            Debug.Print("Processing commands.");

            try
            {
                DeviceCommand dc = null;
                //lock (DcClient)
                {
                    if (BeforeCommand != null)
                    {
                        BeforeCommand(this, new CommandEventArgs(null));
                    }
                    dc = DcClient.GetCommand(DeviceData);
                    //System.Threading.Thread.Sleep(100);
                }
                if (dc != null)
                {
                    
                    //PreProcessMessage(dc);
                    bool rv = false;
                    string code = dc.parameters[EquipmentParameter] as string;
                    if (code != null)
                    {

                        EquipmentEngine e = FindDevice(code);
                        if (e != null)
                        {
                            //lock (DcClient)
                            {
                                rv = e.OnCommand(dc);
                            }
                        }
                        else
                        {
                            if (DeviceCommand != null)
                            {
                                rv = DeviceCommand(this, new CommandEventArgs(dc));
                            }
                        }
                    }
                    DcClient.UpdateCommandStatus(DeviceData, dc.id, new CommandStatus()
                    {
                        status = rv ? CommandStatus.Completed : CommandStatus.Failed,
                        result = null
                    });


                }
                
                if (AfterCommand != null)
                {
                    AfterCommand(this, new CommandEventArgs(dc));
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                Debug.Print(DateTime.Now.ToString() + ": Connection lost. Trying to reconnect.");

                bool doDisconnect = false;

                if (Error != null)
                {
                    doDisconnect = Error(this, new ErrorEventArgs(ex));
                }

                if (doDisconnect)
                {
                    IsConnected = false;
                    IsOnline = false;
                    if (Disconnected != null)
                    {
                        Disconnected(this, new EventArgs());
                    }
                    UnregisterEquipment();
                }
                
                //Connect();
                return false;
                //throw;
            }

        }

        /// <summary>
        /// Sends a notification to the DeviceHive network
        /// </summary>
        /// <param name="notification">Notification to be sent</param>
        /// <returns>True if the notification succeeded</returns>
        /// <remarks>This function can be used by implementers to send device-specific notifications.</remarks>
        /// <example>
        /// The following example shows how to send a notification.
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
        ///         DeviceStatusNotification dn = new DeviceStatusNotification("test notification - " + e.Command.command);
        ///         return SendNotification(dn);
        ///     }
        /// }
        /// </code>
        /// </example>
        virtual public bool SendNotification(INotification notification)
        {
            bool rv = true;
            if (BeforeNotification != null)
            {
                BeforeNotification(this, new NotificationEventArgs(notification));
            }
            if (rv)
            {
                //lock (DcClient)
                {
                    rv = DcClient.PostNotification(DeviceData, notification);
                }
            }
            if (AfterNotification != null)
            {
                AfterNotification(this, new NotificationEventArgs(notification));
            }
            
            return rv;
        }
    }
}
