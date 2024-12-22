using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class SetTableTemp(float temperature, DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			Device.SetTrayTemperature(temperature);
			Result = $"Table temperature set to {_temperature}.";
		}

		public override string Description => $"Set table temperature to {_temperature}.";

		private readonly float _temperature = temperature;
	}
}
