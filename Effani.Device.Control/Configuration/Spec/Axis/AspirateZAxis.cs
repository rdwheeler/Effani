using System;
using System.Linq;

namespace Effani.Device.Control
{
    public class AspirateZAxis : AxisSpec
    {
        public AspirateZAxis(Axis axis, int board, int port)
            : base(axis, board, port)
        {
            Maximum = 90;
            MaxSpeed = 600;
            StepsPerMillimeter = 1260f;
            Driver = new DriverSpec(Label)
            {
                Current = 1500,
                Direction = Direction.Backwards,
                Endstop = new EndstopSpec(Label)
                {
                    SensorPort = $"{(board > 0 ? board + "." : null)}io{port}.in",
                    Signal = PinSignal.High,
                    Location = EndstopLocation.LowEnd,
                    FirstHomingSpeed = 1000
                }
            };
        }
    }
}
