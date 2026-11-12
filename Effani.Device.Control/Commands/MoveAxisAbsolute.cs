using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class MoveAxisAbsolute : MoveAbsolute
	{
		public MoveAxisAbsolute(Axis axis, decimal position, EffaniDeviceControl device) : base(position, device)
		{
			_device = device;
			if (!_device.Axes.TryGetValue(axis, out var axisSpec))
				throw new ApplicationException($"Unknown axis {axis} in {nameof(MoveAxisAbsolute)}");

			AxisSpec = axisSpec;
		}
		private readonly EffaniDeviceControl _device;

	}
}
