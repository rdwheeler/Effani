using Effani.Device.Control;

namespace Effani.Device.Process
{
	public class EmptyAspirateKoh(DeviceControl device) : Command(device)
	{

		protected override string Run()
		{
			var axisSpec = Device.Axes[Axis.AspirateKoh] as AspirateAxis;
			Device.Move(Axis.AspirateKoh, Device.Configuration.AspirateKohSyringeHomePosition);
			Result =  $"{axisSpec.Label} emptied";
			return Result;
		}

		public override string Result { get; protected set; } = $"KOH emptied";
	}
}
