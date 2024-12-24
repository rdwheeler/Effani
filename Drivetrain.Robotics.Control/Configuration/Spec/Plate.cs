using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class Plate : Spec
	{
		public Plate(Coordinate index) : base($"Plate {index.X}.{index.Y}")
		{
			Index = index;
			var plateRow = index.Y switch
			{
				1 => "A",
				0 => "B",
				_ => index.Y.ToString()
			};

			Label = $"Plate {plateRow}{index.X + 1}";
		}

		public Coordinate Index { get; } // from left to right and top to bottom, this plate's position

		public List<Well> Wells { get; } = new();
	}
}
