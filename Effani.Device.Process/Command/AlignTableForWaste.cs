using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignTableForWaste(DeviceControl device) : Command(device)
	{

		protected override string Run()
		{
			var x = Device.Configuration.WasteLocation.X;
			var y = Device.Configuration.WasteLocation.Y;
			Device.TableGoTo(x, y);
			Result = $"Table aligned for waste";
			return Result;
		}

		public override string Result { get; protected set; } = $"Table aligned with waste";
	}
}
