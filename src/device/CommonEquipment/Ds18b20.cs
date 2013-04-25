using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHI.Premium.Hardware;
using System.Collections;


namespace DeviceHive.CommonEquipment
{
    /// <summary>
    /// Dallas Semiconductors DS-18B20 digital thermo sensor
    /// </summary>
    public class Ds18b20 : ITempSensor
    {

        private byte[] Address;
        private OneWire OneWireBus;

        private const byte MatchROM = 0x55;
        private const byte StartTemperatureConversion = 0x44;
        private const byte ReadScratchPad = 0xBE;


        /// <summary>
        /// Constructs DS-18B20 object for a given 1-wire bus and a device sequence number. 
        /// </summary>
        /// <param name="bus">1-wire bus to whish the sensor is attached</param>
        /// <param name="index">sequence index of a bus element</param>
        public Ds18b20(OneWire bus, int index)
        {
            if (bus.AcquireEx() < 0)
            {
                throw new InvalidOperationException("Invalid OneWire bus.");
            }
            OneWireBus = bus;
            ArrayList devices = GetDevices();
            if (index >= devices.Count)
            {
                throw new IndexOutOfRangeException("Invalid device number.");
            }
            Address = (byte[])devices[index];
        }

        /// <summary>
        /// Constructs DS-18B20 object for a given 1-wire bus and a device address
        /// </summary>
        /// <param name="bus">1-wire bus to whish the sensor is attached</param>
        /// <param name="address">device address</param>
        public Ds18b20(OneWire bus, byte[] address)
        {
            if (bus.AcquireEx() < 0)
            {
                throw new InvalidOperationException("Invalid OneWire bus.");
            }
            OneWireBus = bus;
            ArrayList devices = GetDevices();
            foreach (byte[] Address in devices)
            {
                if (Address.Compare(address))
                {
                    this.Address = Address;
                    return;
                }
            }
            throw new InvalidOperationException("Device with the specified address is not present in the bus.");
        }

        protected ArrayList GetDevices()
        {
            ArrayList devices;
            int attempt = 0;
            do
            {
                devices = OneWireBus.FindAllDevices();
                // Something wrong with 1-wire in .NET MF 4.2, up to 30 attempts are needed to find devices
            } while (devices.Count == 0 && attempt++ < 100);
            if (devices.Count == 0)
            {
                throw new IndexOutOfRangeException("No any device on OneWire bus.");
            }
            return devices;
        }

        /// <summary>
        /// Select command
        /// </summary>
        /// <returns>True if successfull; false - otherwise</returns>
        private bool Select()
        {
            if (OneWireBus.TouchReset() > 0)
            {
                OneWireBus.WriteByte(MatchROM);
                for (byte i = 0; i < Address.Length; i++)
                {
                    OneWireBus.WriteByte(Address[i]);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns current temperature
        /// </summary>
        /// <returns>Temperature value in Celsius</returns>
        public float GetTemperature()
        {
            float rv = float.MinValue;
            lock (OneWireBus)
            {
                if (Select())
                {
                    OneWireBus.WriteByte(StartTemperatureConversion);

                    while (OneWireBus.ReadByte() == 0);

                    if (Select())
                    {
                        OneWireBus.WriteByte(ReadScratchPad);

                        ushort ut = (byte)OneWireBus.ReadByte();
                        ut |= (ushort)(OneWireBus.ReadByte() << 8);
                        rv = ut / 16f;
                    }
                }

            }
            return rv;
        }
    }
}
