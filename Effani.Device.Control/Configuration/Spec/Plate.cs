using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class Plate(Coordinate index) : Spec($"Plate {index.X}.{index.Y}")
	{
		public Coordinate Index { get; } = index; // from left to right and top to bottom, this plate's position

		public List<Well> Wells { get; } = new();
	}
}
