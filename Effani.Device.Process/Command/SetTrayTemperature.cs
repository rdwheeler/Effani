using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class SetTrayTemperature(float temperature, DeviceControl device) : Command(device)
	{
		public override void Run()
		{
			Result = Device.SetTrayTemperature(_temperature);
		}

		private readonly float _temperature = temperature;
	}
	public class TurnOnTrayHeat(DeviceControl device) : SetTrayTemperature(device.Configuration.TrayTemperature, device)
	{
	}
	public class TurnOffTrayHeat(DeviceControl device) : SetTrayTemperature(0, device)
	{
	}
}
