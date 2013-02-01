using DeviceHive;
using DeviceHive.CommonEquipment;
using GHI.Premium.Hardware;
using GHI.Premium.Net;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Net.NetworkInformation;
using System;

namespace EmxDevice
{

    public class PrototypeDevice : DeviceEngine
    {
        private const string NetMask = "255.255.255.0";

        private SignalGenerator BlinkingLed;
        private EthernetBuiltIn Ethernet;
        private OneWire OneWireBus;
        private const int RequestTimeout = 120000;
        private const int WatchDogTimeout = 300000;
        private float LastTemp;
        private const float TempRange = 0.2F;

        public PrototypeDevice()
        {
            DcClient = new DeviceHive.HttpClient(Resources.GetString(Resources.StringResources.CloudUrl), DateTime.MinValue, RequestTimeout);

            Initializing += new ConnectEventHandler(PreInit);
            Connecting += new ConnectEventHandler(PreConnect);
            Connected += new ConnectEventHandler(PostConnect);
            BeforeCommand += new CommandEventHandler(PreProcessCommand);
            AfterCommand += new CommandEventHandler(PostProcessCommand);
            BeforeNotification += new NotificationEventHandler(PreProcessNotification);
            AfterNotification += new NotificationEventHandler(PostProcessNotification);
            Disconnected += new SimpleEventHandler(OnDisconnect);
            LastTemp = 0.0f;
        }

        private bool PreInit(object sender, EventArgs e)
        {
            BlinkingLed = new SignalGenerator(EMX.Pin.IO17, false, 2);
            Blink(100);
            OneWireBus = new OneWire(new OutputPort(EMX.Pin.IO26, false));
            return true;
        }

        private bool PreConnect(object sender, EventArgs e)
        {
            Blink(100);
            
            if (StartClient(String.Empty) != null)
            {
                return true;
            }
            return false;
        }

        protected override Device CreateDeviceData()
        {
            Debug.Print("Initializing device data");
            
            Device dd = new Device()
            {
                deviceClass = new DeviceClass()
                {
                    name = Resources.GetString(Resources.StringResources.DeviceClass),
                    version = Resources.GetString(Resources.StringResources.Version)
                },
                id = new Guid(Resources.GetBytes(Resources.BinaryResources.guid)),
                key = Resources.GetString(Resources.StringResources.Key),
                name = Resources.GetString(Resources.StringResources.DeviceName),
                network = new DeviceNetwork()
                {
                    name = Resources.GetString(Resources.StringResources.Network),
                    description = Resources.GetString(Resources.StringResources.NetworkDesc)
                },
                status = DeviceStatus.OK,
                
            };
            
            Debug.Print("Done initializing device data.");

            
            
            return dd;
        }

        protected override void CreateEquipment()
        {
            Debug.Print("Initializing equipment.");

            Switch led1 = new Switch(this, EMX.Pin.IO9, "LED1", false);
            Ds18b20 SensorDevice = new Ds18b20(OneWireBus, 0);
            TempSensor ts = new TempSensor(this, "TS1", SensorDevice);

            DeviceData.equipment = new Equipment[]
            {
                led1,
                ts
            };
            Debug.Print("Initializing equipment done.");
        }

        private bool PostConnect(object sender, EventArgs e)
        {
            BlinkingLed.Set(true);

            GHI.Premium.Hardware.LowLevel.Watchdog.Enable(WatchDogTimeout);
            return true;
        }

        public float Abs(float f)
        {
            return f < 0.0F ? -f : f;
        }

        public override bool ProcessCommands()
        {
            TempSensor ts = DeviceData.equipment[1] as TempSensor;
            float temp = ts.GetTemperature();
            if (Abs(temp - LastTemp) > TempRange)
            {
                ts.NotifyTemperature();
            }
            return base.ProcessCommands();
        }

        private void PreProcessCommand(object sender, CommandEventArgs e)
        {
            BlinkingLed.Set(false);
        }

        private void PostProcessCommand(object sender, CommandEventArgs e)
        {
            GHI.Premium.Hardware.LowLevel.Watchdog.ResetCounter();
            BlinkingLed.Set(true);
        }

        private void PreProcessNotification(object sender, NotificationEventArgs e)
        {
            BlinkingLed.Set(false);
        }

        private void PostProcessNotification(object sender, NotificationEventArgs e)
        {
            GHI.Premium.Hardware.LowLevel.Watchdog.ResetCounter();
            BlinkingLed.Set(true);
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            BlinkingLed.Set(false);
        }

        private NetworkInterface StartClient(string StaticIP)
        {
            Ethernet = new EthernetBuiltIn();
            Ethernet.Open();

            if (!Ethernet.IsCableConnected)
            {
                Debug.Print("Network cable is not connected!");
            }

            NetworkInterface ni = Ethernet.NetworkInterface;

            if (StaticIP == string.Empty)
            {
                Debug.Print("Starting DHCP client.");
                if (!ni.IsDhcpEnabled)
                {
                    ni.EnableDhcp();
                }
                else
                {
                    ni.RenewDhcpLease();
                }
                Debug.Print("DHCP client started.");
            }
            else
            {
                Debug.Print("Using static IP.");
                ni.EnableStaticIP(StaticIP, NetMask, string.Empty);
                Debug.Print("Static IP enabled.");
            }
            ni.EnableDynamicDns();
            NetworkInterfaceExtension.AssignNetworkingStackTo(Ethernet);
            Debug.Print("IP address is: " + ni.IPAddress);

            return ni;
        }

        private void Blink(uint ms)
        {
            uint[] blink = new uint[] { ms * 1000, ms * 1000 };
            BlinkingLed.Set(true, blink, 0, blink.Length, true);
        }
    }
}
