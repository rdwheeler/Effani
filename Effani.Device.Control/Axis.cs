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
		AspirateKoh,
		[Display(ShortName = "W")]
		AspirateKohZ,
		[Display(ShortName = "A")]
		DnaReagent1,
		[Display(ShortName = "B")]
		Koh1,
		[Display(ShortName = "C")]
		Nadh1,
		[Display(ShortName = "D")]
		Basal1,
        [Display(ShortName = "'e")]
        AspirateFluid,
        [Display(ShortName = "'f")]
        AspirateFluidZ,
        [Display(ShortName = "'g")]
		DnaReagent2,
		[Display(ShortName = "'h")]
		Koh2,
		[Display(ShortName = "'i")]
		Nadh2,
		[Display(ShortName = "'j")]
		Basal2,
	}
}
