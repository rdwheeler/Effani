using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class AlignWellToAxis : MoveTableAbsolute
	{

		public AlignWellToAxis(Axis axis, Well well, AbalDeviceControl device) : base(device)
		{
			_axis = axis;
			_well = well;
			var locatedAxis = LocatedAxis;
			if(locatedAxis is null)
				throw new ApplicationException($"{_axis} not found as locatable axis");

			var x = locatedAxis.Location.X - _well.Location.X;
			var y = locatedAxis.Location.Y - _well.Location.Y;
			Coordinate = new Coordinate(x, y);
		}

		public override string Description => $"Align {_well?.Label} with {LocatedAxis?.Label}.";
		public LocatedAxis LocatedAxis
		{
			get
			{
				if(!(Device as AbalDeviceControl).Axes.TryGetValue(_axis, out var axisSpec))
					return null;
				return axisSpec as LocatedAxis;
			}
		}

		private Axis _axis;
		private readonly Well _well;
	}
}
