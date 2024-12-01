using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignWellToAxis(Axis axis, Well well, DeviceControl device) : Command(device)
	{

		public override void Run()
		{
			var axisSpec = Device.Axes[_axis] as LocatedAxis;
			if (axisSpec is null)
			{
				Result = $"{_axis} not found as locatable axis";
				return;
			}
			var x = axisSpec.Location.X - _well.Location.X;
			var y = axisSpec.Location.Y - _well.Location.Y;
			Device.Move(Axis.TableX, x);
			Device.Move(Axis.TableY, y);
			Result = $" {_well.Label} aligned with {axisSpec.Label}";
		}
		private Axis _axis { get; } = axis;
		private readonly Well _well = well;
	}
}
