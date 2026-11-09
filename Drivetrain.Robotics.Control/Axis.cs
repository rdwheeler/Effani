using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Drivetrain.Robotics.Control
{
	public enum Axis
	{
		[Display(ShortName = "X", Name = "Table X")]
		TableX,
		[Display(ShortName = "Y", Name = "Table Y")]
		TableY,
		[Display(ShortName = "U", Name = "Aspirate")]
		Aspirate,
		[Display(ShortName = "V", Name = "DNA Reagent")]
		DnaReagent,
		[Display(ShortName = "W", Name = "KOH")]
		Koh,
		[Display(ShortName = "A", Name = "NADH")]
		Nadh,
		[Display(ShortName = "B", Name = "Basal")]
		Basal,
        //[Display(ShortName = "D", Name = "Aspirate Fluid")]
        //AspirateFluid,
	}
}
