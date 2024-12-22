using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class TableToTrayLoadingLocation(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as AbalDeviceControl).TableGoTo(Device.Configuration.TrayLoadingLocation);
			Result =  $"Table moved to tray loading location";
		}

		public override string Description => $"Moved table to tray loading location.";
	}
}
