using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class TableToTrayLoadingLocation(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as AbalDeviceControl).TableGoTo(Device.Configuration.TrayLoadingLocation);
		}

		public override string Description => $"Moved table to tray loading location.";
	}
}
