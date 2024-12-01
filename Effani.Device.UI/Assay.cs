using Effani.Device.Control;
using Effani.Device.Process;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static Effani.Device.Control.DeviceControl;

namespace Effani.Device.UI
{
	public class Assay : IDisposable
    {
		public Assay(DeviceControl device, int numberOfTests, ProcessEngine engine)
		{
			Device = device;
			Engine = engine;
			CreatedOn = DateTime.Now;
			BaselineTimer = new Timer(Config.BaselineProcessingDwell) { AutoReset = false };
			BaselineTimer.Elapsed += BaselineTimer_Elapsed;
			var currentTest = 0;
			foreach(var well in Config.DeviceSpecification.Plates.SelectMany(p => p.Wells))
			{
				if (currentTest == numberOfTests + 6)
					break;
				var result = new TestWell(Config, well, TestMediaTimer_Elapsed, KohTimer_Elapsed, DnaTimer_Elapsed);
				WellTests.Add(result);
				currentTest++;
			}       
			DnaBaselineWells = WellTests.Take(4).ToList();
			NadhBaselineWells = WellTests.Skip(DnaBaselineWells.Count()).Take(2).ToList();
			TestWells1 = WellTests.Skip(_baselineWellCount).Take(_testWellCount / 2).ToList();
			TestWells2 = WellTests.Skip(_baselineWellCount + TestWells1.Count()).ToList();
			AllTestWells.AddRange(TestWells1);
			AllTestWells.AddRange(TestWells2);
	}

		public void DoPrepToTimerStart()
		{
			_doingPrepToBaselineTimerStart = true;

			try
			{
				StatusMessages.Add($"Preparing assay...");
				foreach(var fluidLine in Device.FluidAxes)
				{
					StatusMessages.Add($"Priming {fluidLine.Label}");
					Engine.AddCommand(new AlignWasteToPump(fluidLine.Axis, Device));
					Engine.AddCommand(new EmitFluid(fluidLine.Axis, Config.FluidPrimingVolume, Device));
				}
				StatusMessages.Add($"Establishing baselines...");
				foreach(var wellTest in DnaBaselineWells)
				{
					StatusMessages.Add($"Dispensing {Config.DnaVolume}µg DNA reagent to {wellTest.Well.Label}");
					Engine.AddCommand(new AlignWellToAxis(Axis.Dna1, wellTest.Well, Device));
					Engine.AddCommand(new EmitFluid(Axis.Dna1, Config.DnaVolume, Device));
				}
				var baselineTimerStarted = DateTime.Now;
				StatusMessages.Add($"Starting DNA baseline timer ({BaselineTimer.Interval / 1000} seconds)");
				BaselineTimer.Start();
				foreach(var wellTest in NadhBaselineWells)
				{
					StatusMessages.Add($"Dispensing {Config.NadhVolume}µg NADH to {wellTest.Well.Label}");
					Engine.AddCommand(new AlignWellToAxis(Axis.Nadh1, wellTest.Well, Device));
					Engine.AddCommand(new EmitFluid(Axis.Nadh1, Config.NadhVolume, Device));
				}

				StatusMessages.Add($"Reading NADH baselines...");
				var reading1 = GetReading(NadhBaselineWells[0].Well, Config.DnaLaserLocation, "Low NADH", new ReadNadh(Device));
				if (_processTerminating)
					return;
				LowNadh = reading1;

				var reading2 = GetReading(NadhBaselineWells[1].Well, Config.DnaLaserLocation, "High NADH", new ReadNadh(Device));
				if (_processTerminating)
					return;
				HighNadh = reading1;

				StatusMessages.Add($"Starting DNA baseline timer ({(DateTime.Now - baselineTimerStarted).Seconds} seconds remaining)");
			}
			catch (Exception ex)
			{
				Terminate($"Error preparing assay: {ex.Message}");
			}
			finally
			{
				_doingPrepToBaselineTimerStart = false;
			}
		}
		private void DoPrepAfterBaselineTimerElapsed()
		{
			try
			{
				if (_processTerminating)
					return;
				while (_doingPrepToBaselineTimerStart)
				{
					if (_processTerminating)
						return;
					Task.Delay(1000);
				}

				var reading1 = GetReading(DnaBaselineWells[0].Well, Config.DnaLaserLocation, "1 µg DNA", new ReadDna(Device));
				if (_processTerminating)
					return;
				Microgram1Dna = reading1;

				var reading2 = GetReading(DnaBaselineWells[1].Well, Config.DnaLaserLocation, "0.25 µg DNA", new ReadDna(Device));
				if (_processTerminating)
					return;
				MicrogramDna25Dna = reading2;

				var reading3 = GetReading(DnaBaselineWells[2].Well, Config.DnaLaserLocation, "0.0625 µg DNA", new ReadDna(Device));
				if (_processTerminating)
					return;
				MicrogramDna0625Dna = reading3;

				var reading4 = GetReading(DnaBaselineWells[3].Well, Config.DnaLaserLocation, "0 µg DNA", new ReadDna(Device));
				if (_processTerminating)
					return;
				Microgram0Dna = reading4;
				StatusMessages.Add($"Assay preparation complete");
			}
			catch (Exception ex)
			{
				Terminate($"Error preparing assay: {ex.Message}");
			}
			finally
			{
				PreparationComplete?.Invoke(this, EventArgs.Empty);				
			}

		}
		public void DoAssay()
		{
			_samplesNotLoaded = false;
			StatusMessages.Add($"Beginning assay...");
		}
		public void Dispose()
		{
			Terminate(null);
			BaselineTimer.Elapsed -= BaselineTimer_Elapsed;
		}

		private float GetReading(Well well, Coordinate location, string readingName, Command readCommand)
		{
			StatusMessages.Add($"Reading {readingName} for {well.Label}");
			Engine.AddCommand(new AlignWellToLocation(location, well, Device));
			var result = Engine.AddCommandAndGetResult(readCommand);
			if (result is null)
			{
				TerminateOnSensorFailure($"Timeout reading {readingName}");
				return 0;
			}
			if (float.TryParse(result, out var value))
				return value;
			TerminateOnSensorFailure($"Invalid {readingName} value read ({result})");
			return 0;
		}
		private void TerminateOnSensorFailure(string message)
		{
			if (Config.TerminateOnSensorFailure)
				Terminate(message);
		}
		private void Terminate(string message)
		{
			_processTerminating = true;
			BaselineTimer.Stop();
			foreach(var well in WellTests)
				well.Dispose();
			StatusMessages.Add(message);
			StatusMessages.Add($"Process terminating");
		}
		private void BaselineTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			DoPrepAfterBaselineTimerElapsed();
		}
		private void TestMediaTimer_Elapsed(object sender, DataArgs<int> e)
		{
			if (_processTerminating)
				return;
			while (_samplesNotLoaded)
			{
				if (_processTerminating)
					return;
				Task.Delay(3000);
			}

			var well = sender as TestWell;
			switch(e.Data)
			{
				case 1:
					StatusMessages.Add($"Adding test media to {well.Label}");
					Engine.AddCommand(new AlignWellToAxis(Axis.TestMedia1, well.Well, Device));
					Engine.AddCommand(new RemoveFluid(Axis.Aspirate1, Config.TestMediaVolume, Device));
					break;
				case 2:
					foreach (var well in TestWells2)
					{
						StatusMessages.Add($"Adding test media to {well.Label}");
						Engine.AddCommand(new AlignWellToAxis(Axis.TestMedia2, well.Well, Device));
						Engine.AddCommand(new EmitFluid(Axis.TestMedia2, Config.TestMediaVolume, Device));
					}
					break;
			}
		}
		private void KohTimer_Elapsed(object sender, DataArgs<int> e)
		{
		}
		private void DnaTimer_Elapsed(object sender, DataArgs<int> e)
		{
		}

		public string AccountName { get; set; }
		public DateTime CreatedOn { get; set; }
        public int NumberOfSamples { get; set; }
		[DisplayName("Low NADH")]
		public float LowNadh { get; set; }
		[DisplayName("High NADH")]
		public float HighNadh { get; set; }
		[DisplayName("1 µg")]
		public float Microgram1Dna { get; set; }
		[DisplayName("0.25 µg DNA")]
		public float MicrogramDna25Dna { get; set; }
		[DisplayName("0.0625 µg DNA")]
		public float MicrogramDna0625Dna { get; set; }
		[DisplayName("0 µg DNA")]
		public float Microgram0Dna { get; set; }

		public List<TestWell> WellTests = new();

		public List<TestWell> DnaBaselineWells { get; }
		public List<TestWell> NadhBaselineWells { get; }

		public List<TestWell> AllTestWells { get; } = new();
		public List<TestWell> TestWells1 { get; }
		public List<TestWell> TestWells2 { get; }
		public Timer BaselineTimer { get; set; }
		public ObservableCollection<string> StatusMessages { get; } = new();
		public DeviceControl Device { get; private set; }
		public ProcessEngine Engine { get; private set; }
		public DeviceConfiguration Config => Device.Configuration;

		public event EventHandler PreparationComplete;
		public event EventHandler Terminated;

		int _baselineWellCount => DnaBaselineWells.Count() + NadhBaselineWells.Count();
		int _testWellCount => WellTests.Count - _baselineWellCount;
		bool _doingPrepToBaselineTimerStart;
		bool _samplesNotLoaded = true;
		bool _processTerminating;
	}
}
