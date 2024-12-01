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

namespace Effani.Device.UI
{
	public class Assay : IDisposable
    {
		public Assay(DeviceControl device, int numberOfTests, ProcessEngine engine)
		{
			Device = device;
			Engine = engine;
			CreatedOn = DateTime.Now;
			BaselineTimer = new Timer(Config.BaselineProcessingDwell * 1000);
			BaselineTimer.Elapsed += BaselineTimer_Elapsed;
			var currentTest = 0;
			foreach(var well in Config.DeviceSpecification.Plates.SelectMany(p => p.Wells))
			{
				if (currentTest == numberOfTests - 1)
					break;
				var result = new WellTest(Config, well, TestMediaTimer_Elapsed, KohTimer_Elapsed, DnaTimer_Elapsed);
				WellTests.Add(result);
			}       
			DnaBaselineWells = WellTests.Take(4).ToList();
			NadhBaselineWells = WellTests.Skip(DnaBaselineWells.Count()).Take(2).ToList();
			TestWells1 = WellTests.Skip(_baselineWellCount).Take(_testWellCount / 2).ToList();
			TestWells2 = WellTests.Skip(_baselineWellCount + TestWells1.Count()).ToList();
	}

		public void DoPrepToTimerStart()
		{
			_doingPrepToBaselineTimerStart = true;

			try
			{
				StatusMessages.Add($"Prapareing assay...");
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
				if (reading1 is null)
					return;
				HighNadh = reading1.Value;

				var reading2 = GetReading(NadhBaselineWells[4].Well, Config.DnaLaserLocation, "High NADH", new ReadNadh(Device));
				if (reading2 is null)
					return;
				HighNadh = reading1.Value;

				StatusMessages.Add($"Starting DNA baseline timer ({(DateTime.Now - baselineTimerStarted).Seconds} seconds remaining)");
			}
			finally
			{
				_doingPrepToBaselineTimerStart = false;
			}
		}
		private void DoPrepAfterBaselineTimerElapsed()
		{
			if(_processTerminating)
				return;
			while (_doingPrepToBaselineTimerStart)
			{
				if (_processTerminating)
					return;
				Task.Delay(1000);
			}

			var reading1 = GetReading(DnaBaselineWells[0].Well, Config.DnaLaserLocation, "1 µg DNA", new ReadDna(Device));
			if (reading1 is null)
				return;
			Microgram1Dna = reading1.Value;

			var reading2 = GetReading(DnaBaselineWells[1].Well, Config.DnaLaserLocation, "0.25 µg DNA", new ReadDna(Device));
			if (reading2 is null)
				return;
			MicrogramDna25Dna = reading2.Value;

			var reading3 = GetReading(DnaBaselineWells[2].Well, Config.DnaLaserLocation, "0.0625 µg DNA", new ReadDna(Device));
			if (reading3 is null)
				return;
			MicrogramDna0625Dna = reading3.Value;

			var reading4 = GetReading(DnaBaselineWells[3].Well, Config.DnaLaserLocation, "0 µg DNA", new ReadDna(Device));
			if (reading4 is null)
				return;
			Microgram0Dna = reading4.Value;
		}
		public void LoadSamples()
		{

		}
		
		private float? GetReading(Well well, Coordinate location, string readingName, Command readCommand)
		{
			StatusMessages.Add($"Reading {readingName} for {well.Label}");
			Engine.AddCommand(new AlignWellToLocation(location, well, Device));
			var result = Engine.AddCommandAndGetResult(readCommand);
			if (result is null)
			{
				Terminate($"Timeout reading {readingName}");
				return null;
			}
			if (float.TryParse(result, out var value))
				return value;

			Terminate($"Invalid {readingName} value read ({result})");
			return null;
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

		public void Dispose()
		{
			Terminate(null);
			BaselineTimer.Elapsed -= BaselineTimer_Elapsed;
		}
		private void BaselineTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			DoPrepAfterBaselineTimerElapsed();
		}
		private void TestMediaTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
		}
		private void KohTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
		}
		private void DnaTimer_Elapsed(object sender, ElapsedEventArgs e)
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

		public List<WellTest> WellTests = new();

		public List<WellTest> DnaBaselineWells { get; }
		public List<WellTest> NadhBaselineWells { get; }

		public List<WellTest> TestWells1 { get; }

		public List<WellTest> TestWells2 { get; }

		public Timer BaselineTimer { get; set; }

		public ObservableCollection<string> StatusMessages { get; } = new();

		public DeviceControl Device { get; private set; }
		public ProcessEngine Engine { get; private set; }

		public DeviceConfiguration Config => Device.Configuration;

		int _baselineWellCount => DnaBaselineWells.Count() + NadhBaselineWells.Count();
		int _testWellCount => WellTests.Count - _baselineWellCount;

		bool _doingPrepToBaselineTimerStart;

		bool _processTerminating;
	}

	public class WellTest : IDisposable
	{
		public WellTest(DeviceConfiguration deviceConfiguration, Well well, ElapsedEventHandler testMediaTimerCallback, ElapsedEventHandler kohTimerCallback, ElapsedEventHandler dnaTimerCallback)
		{
			_dnaTimerCallback = dnaTimerCallback;
			_kohTimerCallback = kohTimerCallback;
			_testMediaTimerCallback = testMediaTimerCallback;
			Well = well;
			TestMediaTimer1h = new Timer(deviceConfiguration.TestMediaTimerPeriod);
			TestMediaTimer2h = new Timer(deviceConfiguration.TestMediaTimerPeriod * 2);
			TestMediaTimer3h = new Timer(deviceConfiguration.TestMediaTimerPeriod * 3);
			TestMediaTimer4h = new Timer(deviceConfiguration.TestMediaTimerPeriod * 4);
			KohTimer = new Timer(deviceConfiguration.KohTimerPeriod);
			DnaTimer = new Timer(deviceConfiguration.DnaTimerPeriod);

			TestMediaTimer1h.Elapsed += _testMediaTimerCallback;
			TestMediaTimer2h.Elapsed += _testMediaTimerCallback;
			TestMediaTimer3h.Elapsed += _testMediaTimerCallback;
			TestMediaTimer4h.Elapsed += _testMediaTimerCallback;
			KohTimer.Elapsed += _kohTimerCallback;
			DnaTimer.Elapsed += _dnaTimerCallback;
		}
		public void Dispose()
		{
			TestMediaTimer1h.Stop();
			TestMediaTimer2h.Stop();
			TestMediaTimer3h.Stop();
			TestMediaTimer4h.Stop();
			KohTimer.Stop();
			DnaTimer.Stop();

			TestMediaTimer1h.Elapsed -= _testMediaTimerCallback;
			TestMediaTimer2h.Elapsed -= _testMediaTimerCallback;
			TestMediaTimer3h.Elapsed -= _testMediaTimerCallback;
			TestMediaTimer4h.Elapsed -= _testMediaTimerCallback;
			KohTimer.Elapsed -= _kohTimerCallback;
			DnaTimer.Elapsed -= _dnaTimerCallback;
		}

		public float Time1Value { get; set; }
		public float Time2Value { get; set; }
		public float Time3Value { get; set; }
		public float Time4Value { get; set; }
		public float DnaValue { get; set; }
		public bool Canceled { get; set; } = false;
		public Well Well { get; private set; }
		public string SampleId { get; set; }

		public Timer TestMediaTimer1h { get; set; }

		public Timer TestMediaTimer2h { get; set; }
		public Timer TestMediaTimer3h { get; set; }
		public Timer TestMediaTimer4h { get; set; }
		public Timer KohTimer { get; set; }
		public Timer DnaTimer { get; set; }

		readonly ElapsedEventHandler _testMediaTimerCallback;
		readonly ElapsedEventHandler _kohTimerCallback;
		readonly ElapsedEventHandler _dnaTimerCallback;
	}
}
