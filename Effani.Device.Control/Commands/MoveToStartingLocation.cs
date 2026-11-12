using Drivetrain.Robotics.Control;
using System;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class MoveToStartingLocation(Axis axisSpec, EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = Device as EffaniDeviceControl;

			if (_axisSpec == null)
				throw new ApplicationException($"Unknown axis {_axis} in {nameof(MoveToStartingLocation)}");

			Device.MoveToStarting(_axisSpec);
		}

		public override string Description => $"Move {_axisSpec?.Label} to starting location.";
		private AxisSpec _axisSpec
		{
			get
			{
				var device = Device as EffaniDeviceControl;
				device.Axes.TryGetValue(_axis, out var axisSpec);
				return axisSpec;
			}
		}
		private readonly Axis _axis = axisSpec;
	}
}
