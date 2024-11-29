using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class DriverSpec : Spec
	{
		public DriverSpec(string label) : base(label)
		{

		}

		public int Current { get; set; } = 800; // milli amps

		public Direction Direction { get; set; } = Direction.Forwards;
		public EndstopSpec Endstop { get; set; }
	}
}
