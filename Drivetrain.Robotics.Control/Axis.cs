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
		//[Display(ShortName = "Z")]
		//Biopsy,
		//[Display(ShortName = "U")]
		//BiopsyEject,
		[Display(ShortName = "Z")]
		AspirateKoh,
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
        [Display(ShortName = "C")]
        AspirateFluid,
        [Display(ShortName = "D")]
        AspirateFluidZ,
		[Display(ShortName = "X")]
		DnaReagent2,
		[Display(ShortName = "Y")]
		Koh2,
		[Display(ShortName = "Z")]
		Nadh2,
		[Display(ShortName = "U")]
		Basal2,
	}
}
