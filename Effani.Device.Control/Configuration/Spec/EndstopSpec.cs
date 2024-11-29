using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class EndstopSpec : Spec
	{
		public EndstopSpec(string driverLabel)
			: base($"{driverLabel} Endstop")
		{
		}
		public string SensorPort { get; set; }
		public PinSignal Signal { get; set; }
		public EndstopLocation Location { get; set; }

		public int FirstHomingSpeed { get; set; } = 600;

		public int SecondHomingSpeed { get; set; } = 300;
	}
}
