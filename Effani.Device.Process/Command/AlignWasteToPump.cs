using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignWasteToPump(Axis axis, DeviceControl device) : Command(device)
	{

		public override void Execute()
		{
			var axisSpec = Device.Axes[_axis] as LocatedAxis;
			if (axisSpec is null)
			{
				Result = $"{_axis} not found as locatable axis";
				return;
			}
			var x = axisSpec.Location.X - Device.Configuration.WasteLocation.X;
			var y = axisSpec.Location.Y - Device.Configuration.WasteLocation.Y;
			Device.Move(Axis.TableX, x);
			Device.Move(Axis.TableY, y);
			Result = $" {axisSpec.Label} aligned with waste";
		}
		private Axis _axis { get; } = axis;
	}
}
