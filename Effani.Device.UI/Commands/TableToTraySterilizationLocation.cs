using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class TableToTraySterilizationLocation(AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			(Device as AbalDeviceControl).TableGoTo(Device.Configuration.TraySterilizationLocation);
		}

		public override string Description => $"Move table to tray sterilization location.";
	}
}
