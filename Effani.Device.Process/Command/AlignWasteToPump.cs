using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignWasteToPump(Axis axis, DeviceControl device) : Command(device)
	{

		protected override string Run()
		{
			var axisSpec = Device.Axes[_axis] as LocatedAxis;
			if (axisSpec is null)
			{
				Result = $"{_axis} not found as locatable axis";
				return Result;
			}
			var x = axisSpec.Location.X - Device.Configuration.WasteLocation.X;
			var y = axisSpec.Location.Y - Device.Configuration.WasteLocation.Y;
			Device.TableGoTo(x, y);
			Result = $"{axisSpec.Label} aligned with waste";
			return Result;
		}

		public override string Result { get; protected set; } = $"Axis aligned with waste";

		private Axis _axis { get; } = axis;
	}
}
