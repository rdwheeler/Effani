using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
	public class EmitFluid(Axis axis, int microLiters, EffaniDeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			var device = Device as EffaniDeviceControl;
			if (axisSpec is null)
				throw new ApplicationException($"Unknown axis {axis} in {nameof(EmitFluid)}");

			if (device.PrimingStopwatches.TryGetValue(Axis, out var stopwatch))
			{
				stopwatch.Reset();
				stopwatch.Start();
			}

			device.EmitFluid(axisSpec, MicroLiters);
		}

		public override string Description => $"Emit {microLiters}μl from {axisSpec.Label}.";

		public Axis Axis { get; } = axis;
		public int MicroLiters { get; } = microLiters;
		private FluidAxis axisSpec
		{
			get
			{
				var device = Device as EffaniDeviceControl;
				device.Axes.TryGetValue(axis, out var axisSpec);
				return axisSpec as FluidAxis;
			}
		}
    }
}
