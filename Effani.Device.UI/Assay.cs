using DevExpress.Utils.Extensions;
using DevExpress.Xpf.CodeView;
using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
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
using static Drivetrain.Robotics.Control.AbalDeviceControl;

namespace Effani.Device.UI
{
	public class Assay : IDisposable
    {
		public Assay(AbalDeviceControl device, ProcessEngine engine)
		{
			Device = device;
			Engine = engine;
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
			DnaStandardsWells = TestWells.Take(4).ToList();
			NadhStandardsWells = TestWells.Skip(DnaStandardsWells.Count()).Take(2).ToList();
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
                if (MainViewModel.LoadStandardFluids)
                {
                    var job = new Job("Dispensing DNA reagent and NADH for standards establishment.");
                    foreach (var wellTest in DnaStandardsWells)
                    {
                        job.Commands.AddRange(
                        [
                            new AlignWellToAxis(Axis.DnaReagent1, wellTest.Well, Device),
                            new EmitFluid(Axis.DnaReagent1, Config.DnaVolume, Device)
                        ]);
                    }
                    foreach (var wellTest in NadhStandardsWells)
                    {
                        job.Commands.AddRange(
                        [
                            new AlignWellToAxis(Axis.Nadh1, wellTest.Well, Device),
                            new EmitFluid(Axis.Nadh1, Config.NadhVolume, Device)
                        ]);
                    }
                    job.Commands.Add(new StartTimer(BaselineTimer, "Standards", Device));
                    Engine.QueueJob(job);
                }

				QueueReadSensor(NadhStandardsWells[0].Well, Config.NadhLaserLocation, "Low NADH", new ReadNadh(Device, (command) => { LowNadh = ParseCommandResult(command); }));
				QueueReadSensor(NadhStandardsWells[1].Well, Config.NadhLaserLocation, "High NADH", new ReadNadh(Device, (command) => { HighNadh = ParseCommandResult(command); }));
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

                QueueReadSensor(DnaStandardsWells[0].Well, Config.DnaLaserLocation, "1 µg DNA", new ReadDna(Device, (command) => { Microgram1Dna = ParseCommandResult(command); }));
				if (_processTerminating)
					return;

                QueueReadSensor(DnaStandardsWells[1].Well, Config.DnaLaserLocation, "0.25 µg DNA", new ReadDna(Device, (command) => { MicrogramDna25Dna = ParseCommandResult(command); }));
				if (_processTerminating)
					return;

                QueueReadSensor(DnaStandardsWells[2].Well, Config.DnaLaserLocation, "0.0625 µg DNA", new ReadDna(Device, (command) => { MicrogramDna0625Dna = ParseCommandResult(command); }));
				if (_processTerminating)
					return;

                QueueReadSensor(DnaStandardsWells[3].Well, Config.DnaLaserLocation, "0 µg DNA", new ReadDna(Device, (command) => {  Microgram0Dna = ParseCommandResult(command); }));
				if (_processTerminating)
					return;

				Engine.QueueCommand(new NotifyComplete((command) => { StandardsComplete?.Invoke(this, EventArgs.Empty); }, "Notify standards establishment complete.", Device));
			}
            catch (IOException ex)
            {
                StatusMessages.Add($"{ex.Message}");
            }
            catch (Exception ex)
			{
				Terminate($"Error preparing assay: {ex.Message}");
			}

		}
		public void Dispose()
		{
			Terminate(null);
			BaselineTimer.Elapsed -= BaselineTimer_Elapsed;
		}

		private void QueueReadSensor(Well well, Coordinate location, string readingName, Command readCommand)
		{
			Engine.QueueJob(new Job($"Reading {readingName} from {well.Label}.", [
				new AlignWellToLocation(location, well, Device),
				readCommand
			]));
		}
		private float ParseCommandResult(ReadSensor command)
		{
			if (float.TryParse(command.Result, out var value))
			{
				return value;
			}
			else
			{
				return -1;
			}
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

            _nadhLock.Wait();

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
                    QueueReadSensor(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device, (command) => 
					{ 
						testWell.TimeValues.Add(0, ParseCommandResult(command));
					}));
                    if (_processTerminating)
                        return;
                }
            }
            catch (Exception ex)
            {
                Terminate($"Error taking NADH readings: {ex.Message}");
            }
            finally
            {
                _nadhLock.Release();
            }
        }
        private void NadhReadingsTimer_Elapsed(object sender, DataArgs<int> e)
		{
			if (_processTerminating)
				return;

			_nadhReadingLock.Wait();

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
                        QueueReadSensor(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device, (command) =>
						{
							testWell.TimeValues.Add(e.Data, ParseCommandResult(command));
						}));
						if (_processTerminating)
                            return;
                        break;
                    case 3:
                        QueueReadSensor(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device, (command) =>
						{
							testWell.TimeValues.Add(e.Data, ParseCommandResult(command));
						}));
						if (_processTerminating)
                            return;
                        break;
                    case 4:
                        QueueReadSensor(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device, (command) =>
						{
							testWell.TimeValues.Add(e.Data, ParseCommandResult(command));
						}));
						if (_processTerminating)
                            return;
                        break;
                    case 5:
                        QueueReadSensor(testWell.Well, Config.NadhLaserLocation, $"NADH {e.Data}", new ReadNadh(Device, (command) =>
						{
							testWell.TimeValues.Add(e.Data, ParseCommandResult(command));
						}));
						if (_processTerminating)
                            return;

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
				_nadhReadingLock.Release();
			}
		}
		private void KohDwellTimer_Elapsed(object sender, DataArgs<int> e)
		{
			_dnaLock.Wait();
            try
            {
                var testWell = sender as TestWell;
                if (MainViewModel.DoKohTimerSteps)
                {
                    TestInDnaPhase?.Invoke(this, new DataArgs<TestWell>(testWell));
                    Axis dnaAxis = testWell.Well.Plate.Index.X == 0 ? Axis.DnaReagent1 : Axis.DnaReagent2;
                    var job = new Job($"Dispensing DNA reagent to {testWell.Label}");
					// remove KOH from well
					job.Commands.AddRange(GetAspirationCommands(FluidType.Koh, testWell, Config.DnaAspirationVolume));
					job.Commands.AddRange(
                    [
                        // add DNA reagent to well
                        new AlignWellToAxis(dnaAxis, testWell.Well, Device),
                        new EmitFluid(dnaAxis, Config.DnaVolume, Device),
                        new StartTimer(testWell.DnaIncubationTimer, $"{testWell.Label} DNA", Device)
                    ]);
                    Engine.QueueJob(job);
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
				_dnaLock.Release();
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
                    QueueReadSensor(testWell.Well, Config.DnaLaserLocation, $"DNA", new ReadDna(Device, (command) => { testWell.DnaValue = ParseCommandResult(command); }));
                    if (_processTerminating)
                        return;
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
		[DisplayName("Low NADH")]
		public float LowNadh { get; set; }
		[DisplayName("High NADH")]
		public float HighNadh { get; set; }
		[DisplayName("1 µg DNA")]
		public float Microgram1Dna { get; set; }
		[DisplayName("0.25 µg DNA")]
		public float MicrogramDna25Dna { get; set; }
		[DisplayName("0.0625 µg DNA")]
		public float MicrogramDna0625Dna { get; set; }
		[DisplayName("0 µg DNA")]
		public float Microgram0Dna { get; set; }

		public List<TestWell> TestWells { get; private set; } = new();

		public List<TestWell> DnaStandardsWells { get; private set; }
		public List<TestWell> NadhStandardsWells { get; private set; }

		public List<TestWell> AllTestWells { get; private set; } = new();
		public List<TestWell> TestWells1 { get; private set; }
		public List<TestWell> TestWells2 { get; private set; }
		public System.Timers.Timer BaselineTimer { get; set; }
		public ObservableCollection<string> StatusMessages { get; } = new();
		public AbalDeviceControl Device { get; private set; }
		public ProcessEngine Engine { get; private set; }
		public DeviceConfiguration Config => Device.Configuration;

		public event EventHandler StandardsComplete;
		public event EventHandler Terminated;
		public event EventHandler<DataArgs<TestWell>> TestInNadhReadingsPhase;
		public event EventHandler<DataArgs<TestWell>> TestInKohPhase;
		public event EventHandler<DataArgs<TestWell>> TestInDnaPhase;
        public event EventHandler<DataArgs<TestWell>> TestComplete;

        int _baselineWellCount => DnaStandardsWells.Count() + NadhStandardsWells.Count();
		int _testWellCount => TestWells.Count - _baselineWellCount;
		bool _doingPrepToBaselineTimerStart;
		bool _processTerminating;
        SemaphoreSlim _nadhLock = new SemaphoreSlim(1, 1);
        SemaphoreSlim _nadhReadingLock = new SemaphoreSlim(1, 1);
		public SemaphoreSlim _dnaLock = new SemaphoreSlim(1, 1);
		SemaphoreSlim _dnaTimerLock = new SemaphoreSlim(1, 1);
	}
}
