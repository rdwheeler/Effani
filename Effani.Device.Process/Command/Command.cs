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
			if(!Device.Configuration.BypassDeviceCommands)
				Run();
			else
				Result = $"{GetType().Name} Bypassed";
		}
		public abstract void Run();
		public DeviceControl Device { get; } = device;
		public string Result { get; protected set; }
	}
}
