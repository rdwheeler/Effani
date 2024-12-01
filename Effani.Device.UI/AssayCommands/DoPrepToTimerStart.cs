using Effani.Device.Control;
using Effani.Device.Process;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Effani.Device.UI
{
	public class DoPrepToTimerStart(Assay assay, DeviceControl device) : Command(device)
	{
		public override void Run()
		{
			_statusMessages.Add($"Prapareing assay...");
			foreach (var fluidLine in Assay.Device.FluidAxes)
			{
				_statusMessages.Add($"Priming {fluidLine.Label}");
				Assay.Engine.AddCommand(new AlignWasteToPump(fluidLine.Axis, Assay.Device));
				Assay.Engine.AddCommand(new EmitFluid(fluidLine.Axis, Assay.Config.FluidPrimingVolume, Assay.Device));
			}
			_statusMessages.Add($"Establishing baselines...");
			foreach (var wellTest in Assay.DnaBaselineWells)
			{
				_statusMessages.Add($"Dispensing {Assay.Config.DnaVolume}µg DNA reagent to {wellTest.Well.Label}");
				Assay.Engine.AddCommand(new AlignWellToAxis(Axis.Dna1, wellTest.Well, Assay.Device));
				Assay.Engine.AddCommand(new EmitFluid(Axis.Dna1, Assay.Config.DnaVolume, Assay.Device));
			}
			_statusMessages.Add($"Starting DNA baseline timer ({Assay.BaselineTimer.Interval / 1000} seconds)");
			Assay.BaselineTimer.Start();
			foreach (var wellTest in Assay.NadhBaselineWells)
			{
				_statusMessages.Add($"Dispensing {Assay.Config.NadhVolume}µg NADH to {wellTest.Well.Label}");
				Assay.Engine.AddCommand(new AlignWellToAxis(Axis.Nadh1, wellTest.Well, Assay.Device));
				Assay.Engine.AddCommand(new EmitFluid(Axis.Nadh1, Assay.Config.NadhVolume, Assay.Device));
			}
			_statusMessages.Add($"Reading NADH baselines...");
			_statusMessages.Add($"Reading Low NADH for {Assay.NadhBaselineWells.First().Well.Label}");
			Assay.Engine.AddCommand(new AlignWellToAxis(Axis.Nadh1, Assay.NadhBaselineWells.First().Well, Assay.Device));
			var resultLow = Assay.Engine.AddCommandAndGetResult(new ReadNadh(Assay.Device));
			if (resultLow is null)
			{
				_statusMessages.Add($"Timeout reading NADH baselines. Process terminating.");
				return;
			}
			if (float.TryParse(resultLow, out var valueLow))
				Assay.LowNadh = valueLow;
			else
				_statusMessages.Add($"Invalid Low NADH value read ({resultLow}). Process terminating.");

			_statusMessages.Add($"Reading High NADH for {Assay.NadhBaselineWells.Last().Well.Label}");
			Assay.Engine.AddCommand(new AlignWellToAxis(Axis.Nadh1, Assay.NadhBaselineWells.Last().Well, Assay.Device));
			var resultHigh = Assay.Engine.AddCommandAndGetResult(new ReadNadh(Assay.Device));
			if (resultHigh is null)
			{
				_statusMessages.Add($"Timeout reading NADH baselines. Process terminating.");
				return;
			}
			if (float.TryParse(resultHigh, out var valueHigh))
				Assay.HighNadh = valueHigh;
			else
				_statusMessages.Add($"Invalid High NADH value read ({resultHigh}). Process terminating.");
		}
		public Assay Assay { get; } = assay;

		ObservableCollection<string> _statusMessages => Assay.StatusMessages;
	}
}
