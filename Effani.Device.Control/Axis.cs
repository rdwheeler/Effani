using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Effani.Device.Control
{
	public enum Axis
	{
		[Display(ShortName = "X")]
		TableX,
		[Display(ShortName = "Y")]
		TableY,
		[Display(ShortName = "Z")]
		Biopsy,
		[Display(ShortName = "U")]
		BiopsyEject,
		[Display(ShortName = "V")]
		AspirateFluid,
		[Display(ShortName = "W")]
		AspirateFluidZ,
		[Display(ShortName = "A")]
		AspirateKoh,
		[Display(ShortName = "B")]
		AspirateKohZ,
		[Display(ShortName = "D")]
		DnaReagent1,
		[Display(ShortName = "C")]
		DnaReagent2,
		[Display(ShortName = "'b")]
		Koh1,
		[Display(ShortName = "'a")]
		Koh2,
		[Display(ShortName = "'d")]
		Nadh1,
		[Display(ShortName = "'c")]
		Nadh2,
		[Display(ShortName = "'f")]
		Basal1,
		[Display(ShortName = "'e")]
		Basal2
	}
}
