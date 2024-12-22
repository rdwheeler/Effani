using Effani.Device.Control;
using System;
using System.Linq;
using System.Reflection.Metadata;

namespace Effani.Device.Process
{
	public class MoveTableAbsolute(AbalDeviceControl device, Coordinate coordinate = null) : Command(device)
	{
		protected override void Run()
		{
			if(Coordinate == null)
				throw new ApplicationException($"{nameof(Coordinate)} cannot be null in {nameof(MoveTableAbsolute)}");

			(Device as AbalDeviceControl).TableGoTo(coordinate.X, coordinate.Y);
			Result = $"Table moved to absolute coordinate {coordinate.X},{Coordinate.Y}.";
		}

		public override string Description => $"Move table to absolute coordinate {coordinate.X},{Coordinate.Y}.";

		public Coordinate Coordinate { get; set; } = coordinate;
	}
}
