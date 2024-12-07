using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignWellToAxis(Axis axis, Well well, DeviceControl device) : Command(device)
	{

		protected override string Run()
        {
            if (!Device.Axes.TryGetValue(axis, out var axisSpec))
                return "Unknown axis";
            var locatedAxis = axisSpec as LocatedAxis;
			if (locatedAxis is null)
			{
				return $"{_axis} not found as locatable axis";
			}
			var x = locatedAxis.Location.X - _well.Location.X;
			var y = locatedAxis.Location.Y - _well.Location.Y;
			Device.TableGoTo(x, y);
			Result =  $"{_well.Label} aligned with {locatedAxis.Label}";
			return Result;
		}

		public override string Result { get; protected set; } = $"Well aligned with axis";

		private Axis _axis { get; } = axis;
		private readonly Well _well = well;
	}
}
