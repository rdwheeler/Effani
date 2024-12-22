using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class MoveAxisAbsolute(Axis axis, float position, AbalDeviceControl device) : MoveAbsolute(position, device)
	{
		protected override void Run()
		{
			if (!_device.Axes.TryGetValue(axis, out var axisSpec))
				throw new ApplicationException($"Unknown axis {Axis} in {nameof(MoveAxisAbsolute)}");

			AxisSpec = axisSpec;
			base.Run();
		}

		public Axis Axis { get; } = axis;
		private readonly AbalDeviceControl _device = device;
	}
}
