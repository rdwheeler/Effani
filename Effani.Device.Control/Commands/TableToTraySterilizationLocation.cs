using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class TableToTraySterilizationLocation(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as AbalDeviceControl).TableGoTo(Device.Configuration.Settings.TraySterilizationLocation);
		}

		public override string Description => $"Move table to tray sterilization location.";
	}
}
