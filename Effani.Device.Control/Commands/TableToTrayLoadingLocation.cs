using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class TableToTrayLoadingLocation(EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as EffaniDeviceControl).TableGoTo(Device.Configuration.Settings.TrayLoadingLocation);
		}

		public override string Description => $"Move table to tray loading location.";
	}
}
