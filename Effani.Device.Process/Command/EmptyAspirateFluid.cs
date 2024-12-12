using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class EmptyAspirateFluid(DeviceControl device) : Command(device)
	{

		protected override string Run()
        {
            var axisSpec = Device.Axes[Axis.AspirateFluid] as AspirateAxis;
            Device.Move(Axis.AspirateFluid, Device.Configuration.AspirateFluidSyringeHomePosition);
            Result = $"{axisSpec.Label} emptied";
            return Result;
        }

		public override string Result { get; protected set; } = $"Aspirate emptied";
	}
}
