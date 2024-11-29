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
		BiopsyPri,
		[Display(ShortName = "U")]
		BiopsySec,
		[Display(ShortName = "V")]
		Aspirate1,
		[Display(ShortName = "W")]
		Aspirate1Z,
		[Display(ShortName = "A")]
		Aspirate2,
		[Display(ShortName = "B")]
		Aspirate2Z,
		[Display(ShortName = "D")]
		Dna1,
		[Display(ShortName = "C")]
		Dna2,
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
