namespace Drivetrain.Robotics.Control
{
	public class MoveSpec(AxisSpec axis, float location)
	{
		public AxisSpec Axis { get; set; } = axis;
		public float location { get; set; } = location;
		public string ToGode()
		{
			return $"{Axis.Designation}{location}";
		}
	}
}
