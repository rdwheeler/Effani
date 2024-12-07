using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class AxisGoto(Axis axis, float location, DeviceControl device) : Command(device)
	{
		protected override string Run()
		{
			Result = Device.GoTo(Axis, location);
			return Result;
		}

		public override string Result { get; protected set; } = $"Axis goto location";

		public Axis Axis { get; } = axis;
		public float Location { get; } = location;
	}
}
