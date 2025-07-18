namespace Drivetrain.Robotics.Control
{
	public class MoveSpec(AxisSpec axis, decimal location)
	{
		public AxisSpec Axis { get; set; } = axis;
		public decimal location { get; set; } = location;
		public string ToGode()
		{
			return $"{Axis.Designation}{location}";
		}
	}
}
