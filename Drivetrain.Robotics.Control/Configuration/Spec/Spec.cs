using System;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public class Spec
	{
		public Spec(string label)
		{
			Label = label;
		}
		public string Label { get; set; }
	}
}
