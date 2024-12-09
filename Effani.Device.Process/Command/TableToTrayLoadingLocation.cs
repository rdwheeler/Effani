using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class TableToTrayLoadingLocation(DeviceControl device) : Command(device)
	{

		protected override string Run()
		{
			Device.TableGoTo(Device.Configuration.TrayLoadingLocation);
			Result =  $"Table moved to tray loading location";
			return Result;
		}

		public override string Result { get; protected set; } = $"Table moved to tray loading location";
	}
}
