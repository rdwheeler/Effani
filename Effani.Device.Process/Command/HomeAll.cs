using Effani.Device.Control;
using System;
using System.Linq;

namespace Effani.Device.Process
{
	public class HomeAll(DeviceControl device) : Command(device)
	{
		protected override string Run()
		{
            foreach (var axisSpec in Device.Axes.Values)
            {
                Device.Home(axisSpec.Axis);
            }
			//Device.HomeAll();
			foreach(var fluidLine in Device.FluidAxes)
			{
				Device.GoTo(fluidLine.Axis, Device.Configuration.FluidSyringeHomePosition);
			}
			Device.GoTo(Axis.AspirateFluid, Device.Configuration.AspirateFluidSyringeHomePosition);
			Device.GoTo(Axis.AspirateKoh, Device.Configuration.AspirateKohSyringeHomePosition);
            Device.GoTo(Axis.AspirateFluidZ, 0);
            Device.GoTo(Axis.AspirateKohZ, 0);
            Device.GoTo(Axis.Biopsy, Device.Configuration.BiospsyLoadingLocation);
			Device.GoTo(Axis.BiopsyEject, Device.Configuration.BiopsyEjectHomePosition);
			Result = $"All axis homed";
			return Result;
		}

		public override string Result { get; protected set; } = $"All axis homed";
	}
}
