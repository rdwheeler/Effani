using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class TableToTraySterilizationLocation(EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as EffaniDeviceControl).TableGoTo(Device.Configuration.Settings.TraySterilizationLocation);
		}

		public override string Description => $"Move table to tray sterilization location.";
	}
}
