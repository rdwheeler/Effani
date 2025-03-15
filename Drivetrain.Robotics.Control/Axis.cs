using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public enum Axis
	{
		[Display(ShortName = "X")]
		TableX,
		[Display(ShortName = "Y")]
		TableY,
		[Display(ShortName = "U")]
		AspirateKohZ,
		[Display(ShortName = "V")]
		DnaReagent1,
		[Display(ShortName = "W")]
		Koh1,
		[Display(ShortName = "A")]
		Nadh1,
		[Display(ShortName = "B")]
		Basal1,
        [Display(ShortName = "D")]
        AspirateFluidZ,
	}
}
