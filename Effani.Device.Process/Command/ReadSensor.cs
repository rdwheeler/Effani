using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public abstract class ReadSensor(DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			Result = device.ReadSensor(SensorDesignator, PauseTime);
		}

		public int PauseTime { get; set; }
		public int SensorDesignator { get; set; }
	}

}
