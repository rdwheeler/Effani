using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class EmptyAspirateFluid : EmptyAspirate
	{
		public EmptyAspirateFluid(AbalDeviceControl device) : base(device)
		{
			AxisSpec = (Device as AbalDeviceControl).Axes[Axis.AspirateFluid] as AspirateAxis;
		}
	}
}
