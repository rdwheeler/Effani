using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class Plate : Spec
	{
		public Plate(Coordinate index) : base(null)
		{
			Location = index;
			var plateRow = index.X switch
			{
				0 => "A",
				1 => "B",
				_ => index.Y.ToString()
			};

			Label = $"{plateRow}{index.Y + 1}";
		}

		public Coordinate Location { get; } // from left to right and bottom to top, this plate's position

		public List<Well> Wells { get; } = new();
	}
}
