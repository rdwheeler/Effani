using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class EmitFluid(Axis axis, int microLiters, AbalDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = Device as AbalDeviceControl;
			if (axisSpec is null)
				throw new ApplicationException($"Unknown axis {axis} in {nameof(EmitFluid)}");

			device.EmitFluid(axisSpec, MicroLiters, 3000);
		}

		public override string Description => $"Emit {microLiters}μl from {axisSpec.Label}.";

		public Axis Axis { get; } = axis;
		public int MicroLiters { get; } = microLiters;
		private FluidAxis axisSpec
		{
			get
			{
				var device = Device as AbalDeviceControl;
				device.Axes.TryGetValue(axis, out var axisSpec);
				return axisSpec as FluidAxis;
			}
		}
    }
}
