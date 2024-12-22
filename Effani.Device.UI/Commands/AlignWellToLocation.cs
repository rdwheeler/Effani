using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AlignWellToLocation(Coordinate location, Well well, AbalDeviceControl device) : MoveTableAbsolute(device)
	{

		protected override void Run()
		{
			Coordinate = _well.Location;
			base.Run();
			Result =  $"{_well.Label} aligned with {_location.X}.{_location.Y}";
		}

		public override string Description => $"Aligned {_well.Label} with {_location.X}.{_location.Y}";

		private readonly Coordinate _location = location;
		private readonly Well _well = well;
	}
}
