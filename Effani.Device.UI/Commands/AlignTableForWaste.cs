using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class AlignTableForWaste(AbalDeviceControl device) : MoveTableAbsolute(device, device.Configuration.WasteLocation)
	{
		public override string Description => $"Align table for waste disposal.";
	}
}
