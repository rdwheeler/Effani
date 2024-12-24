using Drivetrain.Robotics.Control;
using System;

namespace Drivetrain.Robotics.Process
{
	public class MoveToStartingLocation(Axis axisSpec, AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = Device as AbalDeviceControl;

			if (_axisSpec == null)
				throw new ApplicationException($"Unknown axis {_axis} in {nameof(MoveToStartingLocation)}");

			Device.MoveToStarting(_axisSpec);
		}

		public override string Description => $"Move {_axisSpec?.Label} to starting location.";
		private AxisSpec _axisSpec
		{
			get
			{
				var device = Device as AbalDeviceControl;
				device.Axes.TryGetValue(_axis, out var axisSpec);
				return axisSpec;
			}
		}
		private readonly Axis _axis = axisSpec;
	}
}
