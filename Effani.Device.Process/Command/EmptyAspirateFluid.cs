using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class EmptyAspirateFluid(DeviceControl device) : Command(device)
	{

		protected override string Run()
		{
			var axisSpec = Device.Axes[Axis.AspirateFluid] as AspirateAxis;
			var x = axisSpec.Location.X - Device.Configuration.WasteLocation.X;
			var y = axisSpec.Location.Y - Device.Configuration.WasteLocation.Y;
			Device.TableGoTo(x,y);
			Device.GoTo(Axis.AspirateFluid, axisSpec.Maximum);
			return $"{axisSpec.Label} emptied";
		}

		public override string Result { get; protected set; } = $"Aspirate emptied";
	}
}
