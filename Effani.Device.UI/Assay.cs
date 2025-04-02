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
			StandardsTimer = new System.Timers.Timer(Config.StandardsProcessingDwell) { AutoReset = false };
			StandardsTimer.Elapsed += StandardsTimer_Elapsed;
		}

		public void Initialize(int numberOfTests)
		{
			var currentTestNumber = 1;
			foreach(var well in Config.DeviceSpecification.Plates.SelectMany(p => p.Wells))
			{
				var test = new TestWell(Config, well, currentTestNumber, BasalIncubationTimer_Elapsed, NadhReadingsTimer_Elapsed, KohDwellTimer_Elapsed, DnaIncubationTimer_Elapsed);
				if(currentTestNumber > Config.StandardsWellCount)
					test.InitializeTimers();
				TestWells.Add(test);
				currentTestNumber++;
				if (currentTestNumber > numberOfTests + Config.StandardsWellCount)
					break;
			}       
			DnaStandardsWells = TestWells.Take(4).ToList();
			NadhStandardsWells = TestWells.Skip(DnaStandardsWells.Count()).Take(2).ToList();
			TestWells1 = TestWells.Skip(Config.StandardsWellCount).Take(_testWellCount / 2).ToList();
			TestWells2 = TestWells.Skip(Config.StandardsWellCount + TestWells1.Count()).ToList();
			AllTestWells.AddRange(TestWells1);
			AllTestWells.AddRange(TestWells2);
		}
		public void EstablishStandards()
		{
			_doingPrepToBaselineTimerStart = true;

			while (!InitialFluidPrimingComplete)
			{
				if (_processTerminating)
					return;
				Task.Delay(1000).Wait();
			}

			try
			{

				if (MainViewModel.SterilizeAndDispenseStandardFluids)
				{
					Engine.QueuePriorityJob(new Job($"Sterilizing sample trays.", [
						new TableToTraySterilizationLocation(Device),
						new RunSterilizationCycle(Device, Config.TraySterilizationTime)
					]));

					if (!IsPrimedDnaReagent || !IsPrimedNadh)
					{
						var primeJob = new Job("Priming DNA reagent and NADH.");
						primeJob.Commands.Add(new AlignTableForWaste(Device));
						if(!IsPrimedDnaReagent)
						{
							for(int x = 1; x <= Config.SecondaryFluidPrimingIterations; x++)
							{
								primeJob.Commands
									.Add(new EmitFluid(Axis.DnaReagent1, Config.SecondaryFluidPrimingVolume, Device));
							}
							IsPrimedDnaReagent = true;
						}
						if (!IsPrimedNadh)
						{
							for (int x = 1; x <= Config.SecondaryFluidPrimingIterations; x++)
							{
								primeJob.Commands
									.Add(new EmitFluid(Axis.Nadh1, Config.SecondaryFluidPrimingVolume, Device));
							}
							IsPrimedNadh = true;
						}
						Engine.QueueJob(primeJob);
					}
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
                    job.Commands.Add(new StartTimer(StandardsTimer, "Standards", Device));
                    Engine.QueueJob(job);

					Engine.QueueJob(new Job($"Reading Low NADH from {NadhStandardsWells[0].Well.Label}.", [
						new AlignWellToLocation(Config.NadhSensorLocation, NadhStandardsWells[0].Well, Device),
						new ReadNadh(Device, (command) => { LowNadh = ParseCommandResult(command); }),
					]));
					Engine.QueueJob(new Job($"Reading High NADH from {NadhStandardsWells[1].Well.Label}.", [
						new AlignWellToLocation(Config.NadhSensorLocation, NadhStandardsWells[1].Well, Device),
						new ReadNadh(Device, (command) => { HighNadh = ParseCommandResult(command); }),
					]));
                }
				else
				{
					Engine.QueueCommand(new StartTimer(StandardsTimer, "Standards", Device));
				}
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
		public void Dispose()
		{
			Terminate(null);
			StandardsTimer.Elapsed -= StandardsTimer_Elapsed;
			StandardsTimer.Dispose();
			foreach(var well in TestWells)
				well.Dispose();
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
			StandardsTimer.Stop();
			foreach(var well in TestWells)
				well.Dispose();
			StatusMessages.Add(message);
			StatusMessages.Add($"Process terminating");
		}
		private void StandardsTimer_Elapsed(object sender, ElapsedEventArgs e)
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

				if (MainViewModel.DoStandardsSteps)
				{
					Engine.QueueJob(new Job($"Reading 1 µg DNA from {DnaStandardsWells[0].Well.Label}.", [
						new AlignWellToLocation(Config.DnaSensorLocation, DnaStandardsWells[0].Well, Device),
						new ReadDna(Device, (command) => { Microgram1Dna = ParseCommandResult(command); }),
					]));
					Engine.QueueJob(new Job($"Reading 0.25 µg DNA from {DnaStandardsWells[1].Well.Label}.", [
						new AlignWellToLocation(Config.DnaSensorLocation, DnaStandardsWells[1].Well, Device),
						new ReadDna(Device, (command) => { Microgram1Dna = ParseCommandResult(command); }),
					]));
					Engine.QueueJob(new Job($"Reading 0.0625 µg DNA from {DnaStandardsWells[2].Well.Label}.", [
						new AlignWellToLocation(Config.DnaSensorLocation, DnaStandardsWells[2].Well, Device),
						new ReadDna(Device, (command) => { MicrogramDna0625Dna = ParseCommandResult(command); }),
					]));
					Engine.QueueJob(new Job($"Reading 0 µg DNA from {DnaStandardsWells[3].Well.Label}.", [
						new AlignWellToLocation(Config.DnaSensorLocation, DnaStandardsWells[3].Well, Device),
						new ReadDna(Device, (command) => { Microgram0Dna = ParseCommandResult(command); }),
					]));
				}

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
                var testWell = sender as TestWell;
				Engine.QueuePriorityCommand(new NotifyComplete((command) => { TestInNadhReadingsPhase?.Invoke(this, new DataArgs<TestWell>(testWell)); }, $"Notify test in NADH for {testWell.Label}", Device));
                if (MainViewModel.DoNadhReadingSteps)
                {
                    var job = new Job($"Dispensing {Config.NadhVolume}μl NADH to {testWell.Label} for NADH readings 1.");
					// remove fluid from well
					job.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.TestMediaAspirationVolume));
					job.Commands.AddRange(
                    [
                        // add NADH to well
                        new AlignWellToAxis(Axis.Nadh1, testWell.Well, Device),
                        new EmitFluid(Axis.Nadh1, Config.NadhVolume, Device),
                    ]);
                    Engine.QueueJob(job);

					Engine.QueueJob(new Job($"Reading NADH {e.Data} from {testWell.Well.Label}.", [
						new AlignWellToLocation(Config.NadhSensorLocation, testWell.Well, Device),
						new ReadNadh(Device, (command) => { testWell.TimeValues.Add(e.Data, ParseCommandResult(command)); }),
						new StartTimer(testWell.NadhReadings2Timer, $"{testWell.Label} NADH Readings 2", Device)
					]));
				}
				else
				{
					Engine.QueuePriorityJob(new Job($"Skipping NADH fluid readings.",
					[
						new StartTimer(testWell.KohDwellTimer, $"{testWell.Label} KOH dwell", Device),
						new NotifyComplete((command) => { TestInKohPhase?.Invoke(this, new DataArgs<TestWell>(testWell)); }, $"Notify test in KOH for {testWell.Label}", Device)
					]));
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

			void TakeReading(DataArgs<int> e, TestWell testWell, System.Timers.Timer nextTimer)
			{
				Engine.QueueJob(new Job($"Reading NADH {e.Data} from {testWell.Well.Label}.", [
					new AlignWellToLocation(Config.NadhSensorLocation, testWell.Well, Device),
					new ReadNadh(Device, (command) => { testWell.TimeValues.Add(e.Data, ParseCommandResult(command)); }),
				]));
				if (nextTimer != null)
					Engine.QueueCommand(new StartTimer(nextTimer, $"{testWell.Label} NADH Readings {e.Data + 1}", Device));
			}

			try
            {
                var testWell = sender as TestWell;
				switch (e.Data)
				{
					case 2:
						TakeReading(e, testWell, testWell.NadhReadings3Timer);
						break;
					case 3:
						TakeReading(e, testWell, testWell.NadhReadings4Timer);
						break;
                    case 4:
						TakeReading(e, testWell, testWell.NadhReadings5Timer);
                        break;
                    case 5:
						TakeReading(e, testWell, null);
						Engine.QueuePriorityCommand(new NotifyComplete((command) => { TestInKohPhase?.Invoke(this, new DataArgs<TestWell>(testWell)); }, $"Notify test in KOH for {testWell.Label}", Device));
						if (!IsPrimedKoh)
						{
							var primeJob = new Job("Priming KOH.");
							primeJob.Commands.Add(new AlignTableForWaste(Device));
							for (int x = 1; x <= Config.SecondaryFluidPrimingIterations; x++)
							{
								primeJob.Commands
									.Add(new EmitFluid(Axis.Koh1, Config.SecondaryFluidPrimingVolume, Device));
							}
							IsPrimedKoh = true;
							Engine.QueueJob(primeJob);
						}
						var job = new Job($"Adding KOH to {testWell.Label}.");
						// remove fluid from well
						job.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.TestMediaAspirationVolume));
						job.Commands.AddRange(
                        [
							// add KOH to well
							new AlignWellToAxis(Axis.Koh1, testWell.Well, Device),
                            new EmitFluid(Axis.Koh1, Config.KohVolume, Device),
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
				Terminate($"Error taking NADH readings: {ex.Message}");
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
				Engine.QueuePriorityCommand(new NotifyComplete((command) => { TestInDnaPhase?.Invoke(this, new DataArgs<TestWell>(testWell)); }, $"Notify test in DNA for {testWell.Label}", Device));
                if (MainViewModel.DoKohTimerSteps)
                {
					var job = new Job($"Dispensing DNA reagent to {testWell.Label}");
					// remove KOH from well
					job.Commands.AddRange(GetAspirationCommands(FluidType.Koh, testWell, Config.KohAspirationVolume));
					job.Commands.AddRange(
                    [
                        // add DNA reagent to well
                        new AlignWellToAxis(Axis.DnaReagent1, testWell.Well, Device),
                        new EmitFluid(Axis.DnaReagent1, Config.DnaVolume, Device),
                        new StartTimer(testWell.DnaIncubationTimer, $"{testWell.Label} DNA", Device)
					]);
                    Engine.QueueJob(job);
                }
                else
				{
					var job = new Job($"Skipping dispensing DNA reagent for {testWell.Label}");
					job.Commands.AddRange(
					[
						new StartTimer(testWell.DnaIncubationTimer, $"{testWell.Label} DNA", Device)
					]);
					Engine.QueueJob(job);
				}

			}
			catch(Exception ex)
			{
				Terminate($"Error dispensing DNA reagent: {ex.Message}");
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
				Engine.QueuePriorityCommand(new NotifyComplete((command) => { TestComplete?.Invoke(this, new DataArgs<TestWell>(testWell)); }, $"Notify test complete for {testWell.Label}", Device));
                if (MainViewModel.DoDnaIncubationTimerSteps)
				{
					Engine.QueueJob(new Job($"Reading DNA from {testWell.Well.Label}.", [
						new AlignWellToLocation(Config.DnaSensorLocation, testWell.Well, Device),
						new ReadDna(Device, (command) => { testWell.DnaValue = ParseCommandResult(command); }),
					]));
                    if (_processTerminating)
                        return;
                    var job = new Job($"Test completed for {testWell.Label}. Removing remaining fluid.");
					// remove KOH from well
					job.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.CleanUpAspirationVolume));
                    Engine.QueueJob(job);
				}

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
                FluidType.General => Axis.AspirateFluidZ,
                FluidType.Koh => Axis.AspirateKohZ
            };
            Command removeCommand = fluidType switch
            {
                FluidType.General => new RemoveFluid(volume, Device),
                FluidType.Koh => new RemoveKoh(volume, Device),
                _ => null
            };
            var ret = new List<Command>();
            ret.Add(new AlignWellToAxis(axis, testWell.Well, Device));
            ret.Add(removeCommand);

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
		public System.Timers.Timer StandardsTimer { get; set; }
		public ObservableCollection<string> StatusMessages { get; } = new();
		public AbalDeviceControl Device { get; private set; }
		public ProcessEngine Engine { get; private set; }
		public DeviceConfiguration Config => Device.Configuration;
		public bool InitialFluidPrimingComplete { get; set; }
		public bool IsPrimedDnaReagent { get; set; }
		public bool IsPrimedNadh { get; set; }
		public bool IsPrimedKoh { get; set; }
		public bool IsPrimedBasal { get; set; }

		public event EventHandler StandardsComplete;
		public event EventHandler Terminated;
		public event EventHandler<DataArgs<TestWell>> TestInNadhReadingsPhase;
		public event EventHandler<DataArgs<TestWell>> TestInKohPhase;
		public event EventHandler<DataArgs<TestWell>> TestInDnaPhase;
        public event EventHandler<DataArgs<TestWell>> TestComplete;

		int _testWellCount => TestWells.Count - (DnaStandardsWells.Count() + NadhStandardsWells.Count());
		bool _doingPrepToBaselineTimerStart;
		bool _processTerminating;
        SemaphoreSlim _nadhLock = new SemaphoreSlim(1, 1);
        SemaphoreSlim _nadhReadingLock = new SemaphoreSlim(1, 1);
		public SemaphoreSlim _dnaLock = new SemaphoreSlim(1, 1);
		SemaphoreSlim _dnaTimerLock = new SemaphoreSlim(1, 1);
	}
}
