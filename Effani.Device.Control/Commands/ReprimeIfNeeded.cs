using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;

namespace Effani.Device.Control
{
    public class ReprimeIfNeeded(Axis axis, AbalDeviceControl device) : Command(device)
    {
        protected override void Run()
        {
            var device = Device as AbalDeviceControl;
            if (axisSpec is null)
                throw new ApplicationException($"Unknown axis {axis} in {nameof(EmitFluid)}");
            _description = $"Re-prime {axis} pump if needed.";

            if (device.PrimingStopwatches.TryGetValue(Axis, out var stopwatch))
                return;

            long idleTime = stopwatch.ElapsedMilliseconds;
            if (idleTime * 60000 < Device.Settings.ReprimePumpInterval)
                return;

            int primingVolume = GetPrimingVolume();

            _description = $"Re-priming {axis} pump ({primingVolume}µg) after a {idleTime}sec idle period.";
            (Device as AbalDeviceControl).TableGoTo(device.Configuration.Settings.WasteLocation);
            device.EmitFluid(axisSpec, primingVolume);
            stopwatch.Reset();
            stopwatch.Start();
        }
        private int GetPrimingVolume()
        {
            return axis switch
            {
                Axis.DnaReagent => Device.Settings.SecondaryDnaFluidPrimingVolume,
                Axis.Koh => Device.Settings.SecondaryKohFluidPrimingVolume,
                Axis.Nadh => Device.Settings.SecondaryNadhFluidPrimingVolume,
                Axis.Basal => Device.Settings.SecondaryBasalFluidPrimingVolume,
                _ => 0,
            };
        }

        public override string Description => _description;

        public Axis Axis { get; } = axis;

        private string _description;
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
