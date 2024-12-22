using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignTableForWaste(AbalDeviceControl device) : MoveTableAbsolute(device.Configuration.WasteLocation, device)
	{
		public override string Description => $"Table aligned for waste disposal.";
	}
}
