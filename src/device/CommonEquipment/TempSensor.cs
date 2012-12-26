using System;
using Microsoft.SPOT;

namespace DeviceHive.CommonEquipment
{
    /// <summary>
    /// Temperature sensor equipment
    /// </summary>
    public class TempSensor : EquipmentEngine
    {
        private const string TempParameter = "temperature";
        private const string DeviceTypeName = "Temperature Sensor";
        private ITempSensor Sensor;
        //private Timer ThreadTimer;
        private float LastTemp;

        private const float DefaultTolerance = 0.1f;
        private const int DefaultPeriod = 10000;

        /// <summary>
        /// Constructs a DeviceHive temperature sensor for a given sensor object
        /// </summary>
        /// <param name="dev">Parent device</param>
        /// <param name="Code">Equipment code</param>
        /// <param name="sensor">Temperature sensor</param>
        public TempSensor(DeviceEngine dev, string Code, ITempSensor sensor)
            : base(dev)
        {
            Debug.Print("Initializing " + Code + " temperature sensor.");
            name = Code;
            code = Code;
            //equipmentType.name = DeviceTypeName;
            type = DeviceTypeName; //v6
            Sensor = sensor;
            LastTemp = float.MinValue;
            Tolerance = DefaultTolerance;
            Period = DefaultPeriod;
            //Debug.Print("Temperature sensor initialized. Tolerance=" + Tolerance.ToString() + " deg, period=" + (Period / 1000).ToString() + "sec.");
        }

        /// <summary>
        /// Performs a device command
        /// </summary>
        /// <param name="cmd">Input command</param>
        /// <returns>False</returns>
        /// <remarks>This equipment does not support commands. It returns False regardless of input.
        /// </remarks>
        public override bool OnCommand(DeviceCommand cmd)
        {
            //float f = Sensor.GetTemperature();
            //return SendNotification(f);
            return false;
        }

        /// <summary>
        /// Registers the equipment
        /// </summary>
        /// <returns>True if the registration succeeded; false - otherwise</returns>
        public override bool Register()
        {
            Debug.Print("Starting sensor timer.");
            //ThreadTimer = new Timer(new TimerCallback(OnTimer), null, 0, Period);
            return true;
        }

        /// <summary>
        /// Unregisters the equipment 
        /// </summary>
        /// <returns>True if unregister succeeded; false - otherwise</returns>
        public override bool Unregister()
        {
            return true;
        }

        /// <summary>
        /// Sends temperature notification
        /// </summary>
        /// <param name="value">Temperature value</param>
        /// <returns>True if the notificatrion has been successfully sent; false - otherwise</returns>
        public virtual bool SendNotification(float value)
        {
            try
            {
                Debug.Print(code + " sensor temperature is now " + value.ToString());
                return base.SendNotification(TempParameter, value);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets current temperature
        /// </summary>
        /// <returns>Temperature value, in Celsius</returns>
        public float GetTemperature()
        {
            float rv = Sensor.GetTemperature();
            Debug.Print(code + " sensor temperature is now " + rv.ToString());
            return rv;
        }

        /// <summary>
        /// Sends temperature notification
        /// </summary>
        /// <returns>True is successfull, false - otherwise</returns>
        public bool NotifyTemperature()
        {
            try
            {
                Debug.Print("start");
                float rv = GetTemperature();
                Debug.Print("get temp done");
                return base.SendNotification(TempParameter, rv);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Timer for temperature measurement
        /// </summary>
        /// <param name="o"></param>
        private void OnTimer(object o)
        {
            lock (Sensor)
            {
                float f = Sensor.GetTemperature();
                if ((f - LastTemp > Tolerance || f - LastTemp < -Tolerance) || (Tolerance == 0.0f))
                {
                    SendNotification(f);
                    LastTemp = f;
                }
            }
        }

        /// <summary>
        /// Gets or sete the temperature tolerance
        /// </summary>
        /// <remarks>
        /// The temperature sensor will send notifications when the current temperature differs from the previous measurement by a specified value.
        /// </remarks>
        public float Tolerance
        {
            get;
            set;
        }

        /// <summary>
        /// Temperature measurement period
        /// </summary>
        /// <remarks>
        /// In milliseconds
        /// </remarks>
        public int Period
        {
            get;
            set;
        }
    }
}
