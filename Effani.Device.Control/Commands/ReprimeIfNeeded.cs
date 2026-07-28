using Drivetrain.Robotics.Control;
using System;
using System.Linq;
using Drivetrain.Robotics.Process;
using System.Diagnostics;

namespace Effani.Device.Control
{
    public class ReprimeIfNeeded : Command
    {
		public ReprimeIfNeeded(Axis axis, AbalDeviceControl device) : base(device)
		{
			Axis = axis;
            _description = $"Re-prime {axis} pump if needed. Current pump idle time: {GetPumpIdleSeconds()} sec.";
		}
		protected override void Run()
        {
            var device = Device as AbalDeviceControl;
            if (axisSpec is null)
                throw new ApplicationException($"Unknown axis {Axis} in {nameof(EmitFluid)}");

            var stopwatch = GetStopwatch();
            if (stopwatch == null)
                return;

            long idleSec = GetPumpIdleSeconds();
            if (idleSec < Device.Settings.ReprimePumpInterval * 60)
                return;

            int primingVolume = GetPrimingVolume();

            _description = $"Re-priming {Axis} pump ({primingVolume}µg) after a {idleSec} sec idle period.";
            (Device as AbalDeviceControl).TableGoTo(device.Configuration.Settings.WasteLocation);
            device.EmitFluid(axisSpec, primingVolume);
            stopwatch.Reset();
            stopwatch.Start();
        }
        private int GetPrimingVolume()
        {
            return Axis switch
            {
                Axis.DnaReagent => Device.Settings.SecondaryDnaFluidPrimingVolume,
                Axis.Koh => Device.Settings.SecondaryKohFluidPrimingVolume,
                Axis.Nadh => Device.Settings.SecondaryNadhFluidPrimingVolume,
                Axis.Basal => Device.Settings.SecondaryBasalFluidPrimingVolume,
                _ => 0,
            };
        }
        private Stopwatch GetStopwatch()
        {
            if ((Device as AbalDeviceControl).PrimingStopwatches.TryGetValue(Axis, out var stopwatch))
                return stopwatch;
            else
                return null;
		}
        private long GetPumpIdleSeconds()
        {
            var stopwatch = GetStopwatch();
			if (stopwatch == null)
				return 0;
			return stopwatch.ElapsedMilliseconds / 1000;
		}

        public override string Description => _description;

        public Axis Axis { get; private set; }

        private string _description;
		private FluidAxis axisSpec
		{
			get
			{
				var device = Device as AbalDeviceControl;
				device.Axes.TryGetValue(Axis, out var axisSpec);
				return axisSpec as FluidAxis;
			}
        }
    }
}
