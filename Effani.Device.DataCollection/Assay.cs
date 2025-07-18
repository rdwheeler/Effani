using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using Effani.Device.Control;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Timers;

namespace Effani.Device.DataCollection
{
	public class Assay : IDisposable
	{
		#region Constructors
		public Assay(AbalDeviceControl device, ProcessEngine engine)
		{
			Device = device;
			Engine = engine;
			CreatedOn = DateTime.Now;
			StandardsWellCount = Settings.StandardsWellCount;
			StandardsTimer = new System.Timers.Timer(Settings.StandardsProcessingDwell) { AutoReset = false };
			StandardsTimer.Elapsed += StandardsTimer_Elapsed;
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			Terminate(null);
			StandardsTimer.Elapsed -= StandardsTimer_Elapsed;
			StandardsTimer.Dispose();
			foreach (var well in TestWells)
				well.Dispose();
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
				Engine.QueuePriorityJob(new Job($"Sterilizing sample trays.", [
					new TableToTraySterilizationLocation(Device),
					new RunSterilizationCycle(Device, Settings.TraySterilizationTime)
				]));

				if (!IsPrimedStandards)
				{
					IsPrimedStandards = true;
					var primeJob = new Job($"Priming DNA reagent ({Settings.SecondaryDnaFluidPrimingVolume}µg) and NADH ({Settings.SecondaryNadhFluidPrimingVolume}µg).");
					primeJob.Commands.Add(new AlignTableForWaste(Device));
					primeJob.Commands.Add(new EmitFluid(Axis.DnaReagent, Settings.SecondaryDnaFluidPrimingVolume, Device));
					primeJob.Commands.Add(new EmitFluid(Axis.Nadh, Settings.SecondaryNadhFluidPrimingVolume, Device));
					Engine.QueueJob(primeJob);
				}
				var job = new Job($"Dispensing DNA reagent ({Settings.DnaVolume}µg) and NADH/Basal ({Settings.NadhVolume}µg/{Settings.BasalVolume}µg) for standards establishment.");
				foreach (var wellTest in DnaStandardsWells)
				{
					job.Commands.AddRange(
					[
						new AlignWellToAxis(Axis.DnaReagent, wellTest.Well, Device),
						new EmitFluid(Axis.DnaReagent, Settings.DnaVolume, Device)
					]);
				}
				foreach (var wellTest in NadhStandardsWells)
				{
					job.Commands.AddRange(
					[
						new AlignWellToAxis(Axis.Nadh, wellTest.Well, Device),
						new EmitFluid(Axis.Nadh, Settings.NadhVolume, Device),
						new AlignWellToAxis(Axis.Basal, wellTest.Well, Device),
						new EmitFluid(Axis.Basal, Settings.BasalVolume, Device)
					]);
				}
				job.Commands.Add(new StartTimer(StandardsTimer, $"Standards ({GetTimeString(Settings.StandardsProcessingDwell)})", Device));
				Engine.QueueJob(job);

				Engine.QueueJob(new Job($"Reading Low NADH from {NadhStandardsWells[0].Well.Label}.", [
					new AlignWellToLocation(Settings.NadhSensorLocation, NadhStandardsWells[0].Well, Device),
					new Pause(Settings.NadhIncubationDwell, Device),
					new ReadNadh(Device, (command) => { Low_NADH = ParseCommandResult(command); }),
				]));
				Engine.QueueJob(new Job($"Reading High NADH from {NadhStandardsWells[1].Well.Label}.", [
					new AlignWellToLocation(Settings.NadhSensorLocation, NadhStandardsWells[1].Well, Device),
					new ReadNadh(Device, (command) => { High_NADH = ParseCommandResult(command); }),
				]));
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
		public void Initialize(int numberOfTests)
		{
			var currentTestNumber = 1;
			foreach (var well in Device.Configuration.DeviceSpecification.Plates.SelectMany(p => p.Wells))
			{
				var test = new TestWell(Settings, well, currentTestNumber, BasalIncubationTimer_Elapsed, NadhReadingsTimer_Elapsed, KohDwellTimer_Elapsed, DnaIncubationTimer_Elapsed);
				if (currentTestNumber > Settings.StandardsWellCount)
					test.InitializeTimers();
				TestWells.Add(test);
				currentTestNumber++;
				if (currentTestNumber > numberOfTests + Settings.StandardsWellCount)
					break;
			}
			DnaStandardsWells = TestWells.Take(4).ToList();
			NadhStandardsWells = TestWells.Skip(DnaStandardsWells.Count()).Take(2).ToList();
			TestWells1 = TestWells.Skip(Settings.StandardsWellCount).Take(_testWellCount / 2).ToList();
			TestWells2 = TestWells.Skip(Settings.StandardsWellCount + TestWells1.Count()).ToList();
			AllTestWells.AddRange(TestWells1);
			AllTestWells.AddRange(TestWells2);
		}
		#endregion

		#region Private methods
		private void BasalIncubationTimer_Elapsed(object sender, DataArgs<int> e)
		{
			if (_processTerminating)
				return;

			_nadhLock.Wait();

			if (_processTerminating)
				return;

			try
			{
				if (!IsPrimedNadh)
				{
					var primingJob = new Job($"Re-priming NADH ({Settings.SecondaryNadhFluidPrimingVolume}µg).");
					primingJob.Commands.Add(new AlignTableForWaste(Device));
					primingJob.Commands.Add(new EmitFluid(Axis.Nadh, Settings.SecondaryNadhFluidPrimingVolume, Device));
					IsPrimedNadh = true;
					Engine.QueueJob(primingJob);
				}
				var testWell = sender as TestWell;
				Engine.QueuePriorityCommand(new NotifyComplete((command) => { TestInNadhReadingsPhase?.Invoke(this, new DataArgs<TestWell>(testWell)); }, $"Notify test in NADH for {testWell.Label}", Device));
				var job = new Job($"Dispensing {Settings.NadhVolume}μl NADH to {testWell.Label} for NADH readings 1.");
				job.Commands.AddRange(
				[
                    // add NADH to well
                    new AlignWellToAxis(Axis.Nadh, testWell.Well, Device),
					new EmitFluid(Axis.Nadh, Settings.NadhVolume, Device),
					new StartTimer(testWell.NadhReadings1Timer, $"{testWell.Label} NADH Readings 1 ({GetTimeString(Settings.NadhReadingsTimerPeriod)})", Device)
				]);
				Engine.QueueJob(job);
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
		private void DnaIncubationTimer_Elapsed(object sender, DataArgs<int> e)
		{
			_dnaTimerLock.Wait();
			try
			{
				var testWell = sender as TestWell;
				Engine.QueueJob(new Job($"Reading DNA from {testWell.Well.Label}.", [
					new AlignWellToLocation(Settings.DnaSensorLocation, testWell.Well, Device),
					new ReadDna(Device, (command) => { testWell.DnaValue = ParseCommandResult(command); }),
				]));
				if (_processTerminating)
					return;
				var job = new Job($"Test completed for {testWell.Label}.");
				// remove KOH from well
				job.Commands.Add(new NotifyComplete((command) => { TestComplete?.Invoke(this, new DataArgs<TestWell>(testWell)); }, $"Notify test complete for {testWell.Label}", Device));
				Engine.QueueJob(job);
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
				//FluidType.General => Axis.AspirateFluid,
				FluidType.Koh => Axis.AspirateKoh
			};
			Command removeCommand = fluidType switch
			{
				//FluidType.General => new AspirateFluid(volume, Device),
				FluidType.Koh => new AspirateKoh(volume, Device),
				_ => null
			};
			var ret = new List<Command>();
			ret.Add(new AlignWellToAxis(axis, testWell.Well, Device));
			ret.Add(removeCommand);

			return ret;
		}
		private string GetTimeString(int count)
		{
			return count == 0 ? null : TimeSpan.FromMilliseconds(count).ToString("c");
		}
		private void KohDwellTimer_Elapsed(object sender, DataArgs<int> e)
		{
			_dnaLock.Wait();
			try
			{
				var testWell = sender as TestWell;
				Engine.QueuePriorityCommand(new NotifyComplete((command) => { TestInDnaPhase?.Invoke(this, new DataArgs<TestWell>(testWell)); }, $"Notify test in DNA for {testWell.Label}", Device));
				if (!IsPrimedDnaReagent)
				{
					var primeJob = new Job($"Re-priming DNA reagent ({Settings.SecondaryDnaFluidPrimingVolume}µg).");
					primeJob.Commands.Add(new AlignTableForWaste(Device));
					primeJob.Commands.Add(new EmitFluid(Axis.DnaReagent, Settings.SecondaryDnaFluidPrimingVolume, Device));
					IsPrimedDnaReagent = true;
					Engine.QueueJob(primeJob);
				}
				var job = new Job($"Dispensing {Settings.DnaVolume}µg DNA reagent to {testWell.Label}");
				// remove KOH from well
				job.Commands.AddRange(GetAspirationCommands(FluidType.Koh, testWell, Settings.KohAspirationVolume));
				job.Commands.AddRange(
				[
                    // add DNA reagent to well
                    new AlignWellToAxis(Axis.DnaReagent, testWell.Well, Device),
					new EmitFluid(Axis.DnaReagent, Settings.DnaVolume, Device),
					new StartTimer(testWell.DnaIncubationTimer, $"{testWell.Label} DNA ({GetTimeString(Settings.DnaTimerPeriod)})", Device)
				]);
				Engine.QueueJob(job);
			}
			catch (Exception ex)
			{
				Terminate($"Error dispensing DNA reagent: {ex.Message}");
			}
			finally
			{
				_dnaLock.Release();
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
					new AlignWellToLocation(Settings.NadhSensorLocation, testWell.Well, Device),
					new ReadNadh(Device, (command) => { testWell.TimeValues.Add(e.Data, ParseCommandResult(command)); }),
				]));
				if (nextTimer != null)
					Engine.QueueCommand(new StartTimer(nextTimer, $"{testWell.Label} NADH Readings {e.Data + 1} ({GetTimeString(Settings.NadhReadingsTimerPeriod)})", Device));
			}

			try
			{
				var testWell = sender as TestWell;
				switch (e.Data)
				{
					case 1:
						TakeReading(e, testWell, testWell.NadhReadings2Timer);
						break;
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
							var primeJob = new Job($"Priming KOH ({Settings.KohVolume}µg).");
							primeJob.Commands.Add(new AlignTableForWaste(Device));
							primeJob.Commands.Add(new EmitFluid(Axis.Koh, Settings.SecondaryKohFluidPrimingVolume, Device));
							IsPrimedKoh = true;
							Engine.QueueJob(primeJob);
						}
						var job = new Job($"Dispensing {Settings.KohVolume}µg KOH to {testWell.Label}.");
						// remove fluid from well
						//job.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Settings.TestMediaAspirationVolume));
						job.Commands.AddRange(
						[
							// add KOH to well
							new AlignWellToAxis(Axis.Koh, testWell.Well, Device),
							new EmitFluid(Axis.Koh, Settings.KohVolume, Device),
						]);
						job.Commands.AddRange(
						[
							// start KOH dwell timer
							new StartTimer(testWell.KohDwellTimer, $"{testWell.Label} KOH dwell ({GetTimeString(Settings.KohTimerPeriod)})", Device)
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
				Engine.QueueJob(new Job($"Reading 1 µg DNA from {DnaStandardsWells[0].Well.Label}.", [
					new AlignWellToLocation(Settings.DnaSensorLocation, DnaStandardsWells[0].Well, Device),
					new ReadDna(Device, (command) => { DNA_1mcg = ParseCommandResult(command); }),
				]));
				Engine.QueueJob(new Job($"Reading 0.25 µg DNA from {DnaStandardsWells[1].Well.Label}.", [
					new AlignWellToLocation(Settings.DnaSensorLocation, DnaStandardsWells[1].Well, Device),
					new ReadDna(Device, (command) => { DNA_025mcg = ParseCommandResult(command); }),
				]));
				Engine.QueueJob(new Job($"Reading 0.0625 µg DNA from {DnaStandardsWells[2].Well.Label}.", [
					new AlignWellToLocation(Settings.DnaSensorLocation, DnaStandardsWells[2].Well, Device),
					new ReadDna(Device, (command) => { DNA_0625mcg = ParseCommandResult(command); }),
				]));
				Engine.QueueJob(new Job($"Reading 0 µg DNA from {DnaStandardsWells[3].Well.Label}.", [
					new AlignWellToLocation(Settings.DnaSensorLocation, DnaStandardsWells[3].Well, Device),
					new ReadDna(Device, (command) => { DNA_0mcg = ParseCommandResult(command); }),
				]));

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
		private void Terminate(string message)
		{
			_processTerminating = true;
			StandardsTimer.Stop();
			foreach (var well in TestWells)
				well.Dispose();
			StatusMessages.Add(message);
			StatusMessages.Add($"Process terminating");
		}
		#endregion

		#region Properties
		[JsonIgnore]
		public List<TestWell> AllTestWells { get; private set; } = new();
		public string AssayDescription { get; set; }
		[JsonIgnore]
		public Settings Settings => Device.Configuration.Settings;
		public DateTime CreatedOn { get; set; }
		[JsonIgnore]
		public AbalDeviceControl Device { get; private set; }
		[JsonIgnore]
		public List<TestWell> DnaStandardsWells { get; private set; }
		[JsonIgnore]
		public ProcessEngine Engine { get; private set; }
		[JsonPropertyName("High_NADH")]
		public float High_NADH { get; set; }
		[JsonIgnore]
		public bool InitialFluidPrimingComplete { get; set; }
		[JsonIgnore]
		public bool IsPrimedBasal { get; set; }
		[JsonIgnore]
		public bool IsPrimedDnaReagent { get; set; }
		[JsonIgnore]
		public bool IsPrimedKoh { get; set; }
		[JsonIgnore]
		public bool IsPrimedNadh { get; set; }
		[JsonIgnore]
		public bool IsPrimedStandards { get; set; }
		[DisplayName("Low NADH")]
		public float Low_NADH { get; set; }
		[DisplayName("DNA 0mcg")]
		public float DNA_0mcg { get; set; }
		[DisplayName("DNA 1mcg")]
		public float DNA_1mcg { get; set; }
		[DisplayName("DNA 0.0625mcg")]
		public float DNA_0625mcg { get; set; }
		[DisplayName("DNA 0.25mcg")]
		public float DNA_025mcg { get; set; }
		[JsonIgnore]
		public List<TestWell> NadhStandardsWells { get; private set; }
		public int NumberOfSamples { get; set; }
		[JsonIgnore]
		public System.Timers.Timer StandardsTimer { get; set; }
		public int StandardsWellCount { get; set; }
		[JsonIgnore]
		public ObservableCollection<string> StatusMessages { get; } = new();
		public List<TestWell> TestWells { get; private set; } = new();
		[JsonIgnore]
		public List<TestWell> TestWells1 { get; private set; }
		[JsonIgnore]
		public List<TestWell> TestWells2 { get; private set; }

		public string Username { get; set; }

		public Guid Id { get; set; } = Guid.NewGuid();
		#endregion

		#region Events
		public event EventHandler StandardsComplete;
		public event EventHandler Terminated;
		public event EventHandler<DataArgs<TestWell>> TestComplete;
		public event EventHandler<DataArgs<TestWell>> TestInDnaPhase;
		public event EventHandler<DataArgs<TestWell>> TestInKohPhase;
		public event EventHandler<DataArgs<TestWell>> TestInNadhReadingsPhase;
		#endregion

		#region Fields
		int _testWellCount => TestWells.Count - (DnaStandardsWells.Count() + NadhStandardsWells.Count());

		SemaphoreSlim _dnaTimerLock = new SemaphoreSlim(1, 1);
		bool _doingPrepToBaselineTimerStart;
		SemaphoreSlim _nadhLock = new SemaphoreSlim(1, 1);
		SemaphoreSlim _nadhReadingLock = new SemaphoreSlim(1, 1);
		bool _processTerminating;
		public SemaphoreSlim _dnaLock = new SemaphoreSlim(1, 1);
		#endregion

		public enum FluidType
		{
			//General = 0,
			Koh = 1
		}
	}
}
