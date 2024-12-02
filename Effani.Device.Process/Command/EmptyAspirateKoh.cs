using Effani.Device.Control;

namespace Effani.Device.Process
{
	public class EmptyAspirateKoh(DeviceControl device) : Command(device)
	{

		protected override string Run()
		{
			var axisSpec = Device.Axes[Axis.AspirateKoh] as AspirateAxis;
			var x = axisSpec.Location.X - Device.Configuration.WasteLocation.X;
			var y = axisSpec.Location.Y - Device.Configuration.WasteLocation.Y;
			Device.TableGoTo(x, y);
			Device.GoTo(Axis.AspirateFluid, axisSpec.Maximum);
			Result =  $"{axisSpec.Label} emptied";
			return Result;
		}

		public override string Result { get; protected set; } = $"KOH emptied";
	}
}
