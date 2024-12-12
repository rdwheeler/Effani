using System;
using System.Linq;

namespace Effani.Device.Control
{
	public class TableAxis : AxisSpec
	{
		public TableAxis(Axis axis, int board, int port, int maximum, EndstopLocation endstopLocation, Direction direction)
			: base(axis, board, port)
		{
			Maximum = maximum;
			StepsPerMillimeter = 3200;
			MaxSpeed = 300;
			Driver = new DriverSpec(Label)
			{
				Current = 1700,
				Direction = direction,
				Endstop = new EndstopSpec($"{Label} Endstop")
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.Low,
					Location = endstopLocation,
					FirstHomingSpeed = 1800
				}
			};
		}
	}
}
