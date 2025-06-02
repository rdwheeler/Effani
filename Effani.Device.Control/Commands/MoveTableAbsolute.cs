using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using System.Reflection.Metadata;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class MoveTableAbsolute(AbalDeviceControl device, Coordinate coordinate = null) : Command(device)
	{
		protected override void Run()
		{
			if(Coordinate == null)
				throw new ApplicationException($"{nameof(Coordinate)} cannot be null in {nameof(MoveTableAbsolute)}");

			(Device as AbalDeviceControl).TableGoTo(Coordinate.X, Coordinate.Y);
		}

		public override string Description => $"Move table to absolute coordinate {Coordinate.X},{Coordinate.Y}.";

		public Coordinate Coordinate { get; set; } = coordinate;
	}
}
