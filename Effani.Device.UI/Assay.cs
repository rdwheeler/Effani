using DevExpress.Utils.Extensions;
using DevExpress.Xpf.CodeView;
using Effani.Device.Control;
using Effani.Device.Process;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Effani.Device.Control.AbalDeviceControl;

namespace Effani.Device.UI
{
	public class Assay : IDisposable
    {
		public Assay(AbalDeviceControl device, ProcessEngine engine)
		{
			Device = device;
			Engine = engine;
			Engine.DataReceived += Engine_DataReceived;
			CreatedOn = DateTime.Now;
			BaselineTimer = new System.Timers.Timer(Config.StandardsProcessingDwell) { AutoReset = false };
			BaselineTimer.Elapsed += BaselineTimer_Elapsed;
		}

		public void Initialize(int numberOfTests)
		{
			var currentTest = 0;
			foreach(var well in Config.DeviceSpecification.Plates.SelectMany(p => p.Wells))
			{
				if (currentTest == numberOfTests + 6)
					break;
				var result = new TestWell(Config, well, currentTest, BasalIncubationTimer_Elapsed, NadhReadingsTimer_Elapsed, KohDwellTimer_Elapsed, DnaIncubationTimer_Elapsed);
				TestWells.Add(result);
				currentTest++;
			}       
			DnaBaselineWells = TestWells.Take(4).ToList();
			NadhBaselineWells = TestWells.Skip(DnaBaselineWells.Count()).Take(2).ToList();
			TestWells1 = TestWells.Skip(_baselineWellCount).Take(_testWellCount / 2).ToList();
			TestWells2 = TestWells.Skip(_baselineWellCount + TestWells1.Count()).ToList();
			AllTestWells.AddRange(TestWells1);
			AllTestWells.AddRange(TestWells2);
		}
		public void ProcessStandards()
		{
			_doingPrepToBaselineTimerStart = true;

            try
			{
				StatusMessages.Add($"Preparing assay...");

                StatusMessages.Add($"Establishing standards...");
                if (MainViewModel.LoadStandardFluids)
                {
                    var job = new Job("Dispensing DNA reagent and NADH for standards establishment.");
                    foreach (var wellTest in DnaBaselineWells)
                    {
                        job.Commands.AddRange(
                        [
                            new AlignWellToAxis(Axis.DnaReagent1, wellTest.Well, Device),
                            new EmitFluid(Axis.DnaReagent1, Config.DnaVolume, Device),
                        ]);
                    }
                    foreach (var wellTest in NadhBaselineWells)
                    {
                        job.Commands.AddRange(
                        [
                            new AlignWellToAxis(Axis.Nadh1, wellTest.Well, Device),
                            new EmitFluid(Axis.Nadh1, Config.NadhVolume, Device),
                            new EmitFluid(Axis.Nadh1, Config.NadhVolume, Device)
                        ]);
                    }
                    job.Commands.Add(new StartTimer(BaselineTimer, "Standards", Device));
                    Engine.QueueJob(job);
                }

				LowNadh = GetCommandsResult(NadhBaselineWells[0].Well, Config.NadhLaserLocation, "Low NADH", new ReadNadh(Device), NadhLaserReadingKey);
				HighNadh = GetCommandsResult(NadhBaselineWells[1].Well, Config.NadhLaserLocation, "High NADH", new ReadNadh(Device), NadhLaserReadingKey);
            }
            catch (IOException ex)
            {
                StatusMessages.Add($"IO Exception: {ex.Message}");
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
		private void TakeReadingsAfterBaselineTimerElapsed()
		{
			try
			{
				if (_processTerminating)
					return;

				while (_doingPrepToBaselineTimerStart)
				{
					if (_processTerminating)
						return;
					Task.Delay(1000).Wait();
				}

                Microgram1Dna = GetCommandsResult(DnaBaselineWells[0].Well, Config.DnaLaserLocation, "1 µg DNA", new ReadDna(Device), nameof(Microgram1Dna));
				if (_processTerminating)
					return;

                MicrogramDna25Dna = GetCommandsResult(DnaBaselineWells[1].Well, Config.DnaLaserLocation, "0.25 µg DNA", new ReadDna(Device), nameof(MicrogramDna25Dna));
				if (_processTerminating)
					return;

                MicrogramDna0625Dna = GetCommandsResult(DnaBaselineWells[2].Well, Config.DnaLaserLocation, "0.0625 µg DNA", new ReadDna(Device), nameof(MicrogramDna0625Dna));
				if (_processTerminating)
					return;

                Microgram0Dna = GetCommandsResult(DnaBaselineWells[3].Well, Config.DnaLaserLocation, "0 µg DNA", new ReadDna(Device), nameof(Microgram0Dna));
				if (_processTerminating)
					return;

				StatusMessages.Add($"Assay preparation complete");
            }
            catch (IOException ex)
            {
                StatusMessages.Add($"{ex.Message}");
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
			StatusMessages.Add($"Beginning assay...");
		}
		public void Dispose()
		{
			Terminate(null);
			BaselineTimer.Elapsed -= BaselineTimer_Elapsed;
			Engine.DataReceived -= Engine_DataReceived;
		}

		private float GetCommandsResult(Well well, Coordinate location, string readingName, Command readCommand)
		{
			var result = Engine.QueueJobForResult(new Job($"Reading {readingName} from {well.Label}.",
			[
				new AlignWellToLocation(location, well, Device),
				readCommand
			]));
            if (result is null)
            {
                TerminateOnSensorFailure($"Timeout reading {readingName} for {well.Label}");
                return -1;
            }
            if (float.TryParse(result, out var value))
            {
                StatusMessages.Add($"{readingName} for {well.Label} is {value}");
                return value;
            }
            TerminateOnSensorFailure($"Invalid {readingName} value read ({result}) for {well.Label}");
            return -1;
		}

		private const string DnaLaserReadingKey = "1.io6.in";
		private const string NadhLaserReadingKey = "2.io6.in";
		private const string HighNadhName = "HighNadh";
		private const string LowNadhName = "LowNadh";
		private void Engine_DataReceived(object sender, DataArgs<KeyValuePair<string, string>> e)
		{
			//var recievedValue = e.Data.Value;
			//var readingTitle = e.Data.Key;
			//var afterReading = recievedValue.Split("reading ")[1];
			//var firstNumber = afterReading.Split(',')[0];
			//if(float.TryParse(firstNumber, out var readingValue))
			//{
			//	if (readingTitle.EndsWith(HighNadhName))
			//	{
			//		HighNadh = readingValue;
			//	}
			//	else if (readingTitle.EndsWith(LowNadhName))
			//	{
			//		LowNadh = readingValue;
			//	}
			//	else
			//	{
			//		var testWell = TestWells.FirstOrDefault(w => w.Label.StartsWith(readingTitle));
			//		testWell.TimeValues.Add(Int32.Parse(readingTitle.Split(':')[1]), readingValue);
			//	}
			//	StatusMessages.Add($"{readingTitle} is {readingValue}");
			//}
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
			foreach(var well in TestWells)
				well.Dispose();
			StatusMessages.Add(message);
			StatusMessages.Add($"Process terminating");
		}
		private void BaselineTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
            TakeReadingsAfterBaselineTimerElapsed();
		}
        public enum FluidType
        {
            General = 0,
            Koh = 1
        }
        private void BasalIncubationTimer_Elapsed(object sender, DataArgs<int> e)
        {
            if (_processTerminating)
                return;

            _nadhReadingsTimerLock.Wait();

            if (_processTerminating)
                return;

            try
            {
                if (MainViewModel.DoNadhReadings)
                {
                    var testWell = sender as TestWell;
                    TestInNadhReadingsPhase?.Invoke(this, new DataArgs<TestWell>(testWell));
                    Axis nadhAxis = testWell.Well.Plate.Index.X == 0 ? Axis.Nadh1 : Axis.Nadh2;
                    var job1 = new Job($"Dispensing {Config.NadhVolume}μl NADH to {testWell.Label} for NADH readings.");
					// remove fluid from well
					job1.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.TestMediaAspirationVolume));
					job1.Commands.AddRange(
                    [
                        // add NADH to well
                        new AlignWellToAxis(nadhAxis, testWell.Well, Device),
                        new EmitFluid(nadhAxis, Config.NadhVolume, Device),
                        new StartTimer(testWell.NadhReadings2Timer, $"{testWell.Label} NADH Readings 2", Device),
                        new StartTimer(testWell.NadhReadings3Timer, $"{testWell.Label} NADH Readings 3", Device),
                        new StartTimer(testWell.NadhReadings4Timer, $"{testWell.Label} NADH Readings 4", Device),
                        new StartTimer(testWell.NadhReadings5Timer, $"{testWell.Label} NADH Readings 5", Device)
                    ]);

                    Engine.QueueJob(job1);
                    var timeReading = GetCommandsResult(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device), $"TimeValue{e.Data}");
                    if (_processTerminating)
                        return;
                    testWell.TimeValues.Add(0, timeReading);
                }
            }
            catch (Exception ex)
            {
                Terminate($"Error taking NADH readings: {ex.Message}");
            }
            finally
            {
                _nadhReadingsTimerLock.Release();
            }
        }
        private void NadhReadingsTimer_Elapsed(object sender, DataArgs<int> e)
		{
			if (_processTerminating)
				return;

			_testMediaTimerLock.Wait();

			if (_processTerminating)
				return;

			try
            {
                var testWell = sender as TestWell;
                if (!MainViewModel.DoTestMediaTimerSteps)
                {
					if (e.Data == 4)
						Engine.QueueCommand(new StartTimer(testWell.KohDwellTimer, $"{testWell.Label} KOH dwell", Device));
                    return;
                }
				switch (e.Data)
				{
					case 2:
                        var timeReading1 = GetCommandsResult(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device));
                        if (_processTerminating)
                            return;
                        testWell.TimeValues.Add(e.Data, timeReading1);
                        break;
                    case 3:
                        var timeReading2 = GetCommandsResult(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device));
                        if (_processTerminating)
                            return;
                        testWell.TimeValues.Add(e.Data, timeReading2);
                        break;
                    case 4:
                        var timeReading3 = GetCommandsResult(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device));
                        if (_processTerminating)
                            return;
                        testWell.TimeValues.Add(e.Data, timeReading3);
                        break;
                    case 5:
                        var timeReading4 = GetCommandsResult(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device));
                        if (_processTerminating)
                            return;
                        testWell.TimeValues.Add(e.Data, timeReading4);

						Axis kohAxis = testWell.Well.Plate.Index.X == 0 ? Axis.Koh1 : Axis.Koh2;
                        var job = new Job($"Adding KOH to {testWell.Label}.");
						// remove fluid from well
						job.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.KohAspirationVolume));
						job.Commands.AddRange(
                        [
							// add KOH to well
							new AlignWellToAxis(kohAxis, testWell.Well, Device),
                            new EmitFluid(kohAxis, Config.KohVolume, Device),
                        ]);
						job.Commands.AddRange(
						[
							// start KOH dwell timer
							new StartTimer(testWell.KohDwellTimer, $"{testWell.Label} KOH dwell", Device)
						]);
						Engine.QueueJob(job);
						break;
				}
			}
			catch (Exception ex)
			{
				Terminate($"Error preparing test media: {ex.Message}");
			}
			finally
			{
				_testMediaTimerLock.Release();
			}
		}
		private void KohDwellTimer_Elapsed(object sender, DataArgs<int> e)
		{
			_kohTimerLock.Wait();
            try
            {
                var testWell = sender as TestWell;
                if (MainViewModel.DoKohTimerSteps)
                {
                    TestInDnaPhase?.Invoke(this, new DataArgs<TestWell>(testWell));
                    StatusMessages.Add($"Adding DNA assay media to {testWell.Label}");
                    Axis dnaAxis = testWell.Well.Plate.Index.X == 0 ? Axis.DnaReagent1 : Axis.DnaReagent2;
                    var job = new Job();
                    // remove KOH from well
                    packet.Commands.AddRange(GetAspirationCommands(FluidType.Koh, testWell, Config.DnaAspirationVolume));
                    packet.Commands.AddRange(
                    [
                        // add DNA reagent to well
                        new AlignWellToAxis(dnaAxis, testWell.Well, Device),
                        new EmitFluid(dnaAxis, Config.DnaVolume, Device),
                        new StartTimer(testWell.DnaIncubationTimer, $"{testWell.Label} DNA", Device)
                    ]);
                    Engine.QueueJob(packet);
                }
                else
                {
                    testWell.DnaIncubationTimer.Start();
                }
                if (!MainViewModel.DoDnaTimerSteps)
                    TestComplete?.Invoke(this, new DataArgs<TestWell>(testWell));

            }
			catch(Exception ex)
			{
				Terminate($"Error preparing DNA: {ex.Message}");
			}
			finally
			{
				_kohTimerLock.Release();
			}
		}
		private void DnaIncubationTimer_Elapsed(object sender, DataArgs<int> e)
		{
			_dnaTimerLock.Wait();
            try
            {
                var testWell = sender as TestWell;
                if (MainViewModel.DoDnaTimerSteps)
                {
                    var reading = GetCommandsResult(testWell.Well, Config.DnaLaserLocation, $"DNA", new ReadDna(Device));
                    if (_processTerminating)
                        return;
                    testWell.DnaValue = reading;
                    var job = new Job($"Test completed for {testWell.Label}. Removing remaining fluid.");
					// remove KOH from well
					job.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.CleanUpAspirationVolume));
                    Engine.QueueJob(job);
                }

				TestComplete?.Invoke(this, new DataArgs<TestWell>(testWell));
			}
			catch (Exception ex)
			{
				Terminate($"Error reading DNA: {ex.Message}");
			}
			finally
			{
				_dnaTimerLock.Release();
			}
        }
        private List<Command> GetAspirationCommands(FluidType fluidType, TestWell testWell, int volume)
        {
            var axis = fluidType switch
            {
                FluidType.General => Axis.AspirateFluid,
                FluidType.Koh => Axis.AspirateKoh
            };
            Command removeCommand = fluidType switch
            {
                FluidType.General => new RemoveFluid(Config.CleanUpAspirationVolume, Device),
                FluidType.Koh => new RemoveKoh(Config.CleanUpAspirationVolume, Device),
                _ => null
            };
            Command emptyCommand = fluidType switch
            {
                FluidType.General => new EmptyAspirateFluid(Device),
                FluidType.Koh => new EmptyAspirateKoh(Device),
                _ => null
            };
            var ret = new List<Command>();
            ret.Add(new AlignWellToAxis(axis, testWell.Well, Device));
            ret.Add(removeCommand);
            ret.Add(new AlignTableForWaste(Device));
            ret.Add(emptyCommand);

            return ret;
        }

        public string AccountName { get; set; }
		public DateTime CreatedOn { get; set; }
        public int NumberOfSamples { get; set; }

		public List<TestWell> TestWells { get; private set; } = new();

		public List<TestWell> DnaBaselineWells { get; private set; }
		public List<TestWell> NadhBaselineWells { get; private set; }

		public List<TestWell> AllTestWells { get; private set; } = new();
		public List<TestWell> TestWells1 { get; private set; }
		public List<TestWell> TestWells2 { get; private set; }
		public System.Timers.Timer BaselineTimer { get; set; }
		public ObservableCollection<string> StatusMessages { get; } = new();
		public AbalDeviceControl Device { get; private set; }
		public ProcessEngine Engine { get; private set; }
		public DeviceConfiguration Config => Device.Configuration;

		public event EventHandler PreparationComplete;
		public event EventHandler Terminated;
		public event EventHandler<DataArgs<TestWell>> TestInNadhReadingsPhase;
		public event EventHandler<DataArgs<TestWell>> TestInKohPhase;
		public event EventHandler<DataArgs<TestWell>> TestInDnaPhase;
        public event EventHandler<DataArgs<TestWell>> TestComplete;

        int _baselineWellCount => DnaBaselineWells.Count() + NadhBaselineWells.Count();
		int _testWellCount => TestWells.Count - _baselineWellCount;
		bool _doingPrepToBaselineTimerStart;
		bool _processTerminating;
        SemaphoreSlim _nadhReadingsTimerLock = new SemaphoreSlim(1, 1);
        SemaphoreSlim _testMediaTimerLock = new SemaphoreSlim(1, 1);
		public SemaphoreSlim _kohTimerLock = new SemaphoreSlim(1, 1);
		SemaphoreSlim _dnaTimerLock = new SemaphoreSlim(1, 1);
	}
}
