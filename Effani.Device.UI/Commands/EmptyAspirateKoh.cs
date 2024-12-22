using Effani.Device.Control;

namespace Effani.Device.Process
{
	public class EmptyAspirateKoh(AbalDeviceControl device) : EmptyAspirate(device)
	{

		protected override void Run()
		{
			AxisSpec = (Device as AbalDeviceControl).Axes[Axis.AspirateKoh] as AspirateAxis;

			base.Run();
		}
	}
}
