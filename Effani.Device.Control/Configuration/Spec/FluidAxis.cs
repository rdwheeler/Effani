using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class FluidAxis : AxisSpec
	{
		public FluidAxis(Axis axis, int board, int port)
			: base(axis, board, port)
		{
			Maximum = 150;
			StepsPerMillimeter = 1259.84f;
			MaxSpeed = 2286;
			Acceleration = 500;
			Driver = new DriverSpec(Label)
			{
				Current = 1500,
				Direction = Direction.Backwards,
				Endstop = new EndstopSpec(Label)
				{
					SensorPort = $"{(board > 0 ? board : null)}.io{port}.in",
					Signal = PinSignal.High,
					Location = EndstopLocation.LowEnd,
					FirstHomingSpeed = 1000
				}
			};
		}
	}
}
