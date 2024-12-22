using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignWellToAxis(Axis axis, Well well, AbalDeviceControl device) : MoveTableAbsolute(device)
	{
		protected override void Run()
		{
			var locatedAxis = LocatedAxis;
			if(locatedAxis is null)
				throw new ApplicationException($"{_axis} not found as locatable axis");

			var x = locatedAxis.Location.X - _well.Location.X;
			var y = locatedAxis.Location.Y - _well.Location.Y;
			Coordinate = new Coordinate(x, y);
			base.Run();
			Result = $"{_well.Label} aligned with {locatedAxis.Label}";
		}

		public override string Description => $"Align {_well?.Label} with {LocatedAxis?.Label}.";

		public LocatedAxis LocatedAxis
		{
			get
			{
				if(!(Device as AbalDeviceControl).Axes.TryGetValue(axis, out var axisSpec))
					return null;
				return axisSpec as LocatedAxis;
			}
		}

		private Axis _axis { get; } = axis;
		private readonly Well _well = well;
	}
}
