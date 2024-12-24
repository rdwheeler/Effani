using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class MoveAxisAbsolute : MoveAbsolute
	{
		public MoveAxisAbsolute(Axis axis, float position, AbalDeviceControl device) : base(position, device)
		{
			_device = device;
			if (!_device.Axes.TryGetValue(axis, out var axisSpec))
				throw new ApplicationException($"Unknown axis {axis} in {nameof(MoveAxisAbsolute)}");

			AxisSpec = axisSpec;
		}
		private readonly AbalDeviceControl _device;

	}
}
