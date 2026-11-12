using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class AlignTableForWaste(EffaniDeviceControl device) : MoveTableAbsolute(device, device.Configuration.Settings.WasteLocation)
	{
		public override string Description => $"Align table for waste disposal.";
	}
}
