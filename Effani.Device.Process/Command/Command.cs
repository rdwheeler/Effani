using Effani.Device.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.Process
{
	public abstract class Command(DeviceControl device)
	{
		public void Execute()
		{
			if(Device.Configuration.BypassDeviceCommands > 0)
			{
				Task.Delay(Device.Configuration.BypassDeviceCommands).Wait();
			}
			else
			{
				Run();
			}
		}

		protected abstract string Run();
		public DeviceControl Device { get; } = device;
		public abstract string Result { get; protected set; }
	}
}
