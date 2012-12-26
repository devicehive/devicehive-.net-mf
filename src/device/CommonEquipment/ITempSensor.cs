
namespace DeviceHive.CommonEquipment
{
    /// <summary>
    /// Temperature sensor interface
    /// </summary>
    public interface ITempSensor
    {
        /// <summary>
        /// Returns current temperature
        /// </summary>
        /// <returns>Temperature value in Celsius</returns>
        float GetTemperature();
    }
}
