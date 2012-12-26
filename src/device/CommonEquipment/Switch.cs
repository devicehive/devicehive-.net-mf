using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace DeviceHive.CommonEquipment
{
    /// <summary>
    /// Definition of a switch equipment
    /// </summary>
    /// <remarks>
    /// Switch represents a varitey of simple 2-state equipment like LEDs, endswitches, buttons, etc.
    /// </remarks>
    public class Switch : EquipmentEngine
    {
        const string StateParameter = "state";
        const string DeviceTypeName = "Switch";

        private OutputPort Led;

        /// <summary>
        /// Event which indicates a switch state change
        /// </summary>
        public event CommandEventHandler Changed;

        /// <summary>
        /// Constructs a switch by given parameters
        /// </summary>
        /// <param name="dev">Parent device</param>
        /// <param name="pin">Device pin number</param>
        /// <param name="Code">Equipment code</param>
        /// <param name="InitialState">Initial state</param>
        public Switch(DeviceEngine dev, Cpu.Pin pin, string Code, bool InitialState) : base(dev)
        {
            Debug.Print("Initializing " + Code + " switch.");
            Led = new OutputPort(pin, InitialState);
            code = Code;
            name = Code;
            type = DeviceTypeName; // v6
            
            Debug.Print("Done initializing switch.");
        }

        /// <summary>
        /// Sets switch value according to input command
        /// </summary>
        /// <param name="cmd">Input command</param>
        /// <returns>True if successful; false - otherwise</returns>
        public override bool OnCommand(DeviceCommand cmd)
        {
            try
            {
                string val = cmd.parameters[StateParameter].ToString();
                int NewState = int.Parse(val);
                SetValue(NewState, cmd);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Registers the switch
        /// </summary>
        /// <returns>True if successfull; false - otherwise</returns>
        public override bool Register()
        {
            return SendNotification(Led.Read() ? 1 : 0);
        }

        /// <summary>
        /// Sends switch change notification
        /// </summary>
        /// <param name="value">switch value</param>
        /// <returns>True if successfull; false - otherwise</returns>
        public virtual bool SendNotification(int value)
        {
            Debug.Print(code + " is now " + (value == 1 ? "ON" : "OFF"));
            return base.SendNotification(StateParameter, value);
        }

        /// <summary>
        /// Sets switch value
        /// </summary>
        /// <param name="i">New value</param>
        /// <param name="cmd">Input command</param>
        private void SetValue(int i, DeviceCommand cmd)
        {
            Led.Write(i != 0);
            SendNotification(i);
            if (Changed != null)
            {
                Changed(this, new CommandEventArgs(cmd));
            }
        }

        /// <summary>
        /// Gets/sets the switch state
        /// </summary>
        public bool State
        {
            get { return Led.Read(); }
            set 
            {
                if (value != Led.Read())
                {
                    SetValue(value ? 1 : 0, null);
                }
            }
        }
    }
}
