using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	public class EmptyAspirateKoh : EmptyAspirate
	{
		public EmptyAspirateKoh(AbalDeviceControl device) : base(device)
		{
			AxisSpec = (Device as AbalDeviceControl).Axes[Axis.AspirateKoh] as AspirateAxis;
		}
	}
}
