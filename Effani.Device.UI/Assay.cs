using DevExpress.Xpf.CodeView;
using Effani.Device.Control;
using Effani.Device.Process;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Effani.Device.Control.DeviceControl;

namespace Effani.Device.UI
{
	public class Assay : IDisposable
    {
		public Assay(DeviceControl device, ProcessEngine engine)
		{
			Device = device;
			Engine = engine;
			Engine.DataReceived += Engine_DataReceived;
			CreatedOn = DateTime.Now;
			BaselineTimer = new System.Timers.Timer(Config.BaselineProcessingDwell) { AutoReset = false };
			BaselineTimer.Elapsed += BaselineTimer_Elapsed;
		}

		public void Initialize(int numberOfTests)
		{
			var currentTest = 0;
			foreach(var well in Config.DeviceSpecification.Plates.SelectMany(p => p.Wells))
			{
				if (currentTest == numberOfTests + 6)
					break;
				var result = new TestWell(Config, well, currentTest, NadhMediaTimer_Elapsed, TestMediaTimer_Elapsed, KohTimer_Elapsed, DnaTimer_Elapsed);
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
		public void DoPrepToTimerStart()
		{
			_doingPrepToBaselineTimerStart = true;

            try
			{
				StatusMessages.Add($"Preparing assay...");

				StatusMessages.Add($"Establishing standards...");
				foreach(var wellTest in DnaBaselineWells)
				{
					StatusMessages.Add($"Dispensing {Config.DnaVolume}µg DNA reagent to {wellTest.Well.Label}");
					Engine.QueueCommands(new CommandPacket(
					[
						new AlignWellToAxis(Axis.DnaReagent1, wellTest.Well, Device),
						new EmitFluid(Axis.DnaReagent1, Config.DnaVolume, Device),
					]));
				}
				var baselineTimerStarted = DateTime.Now;
				StatusMessages.Add($"Starting DNA baseline timer ({BaselineTimer.Interval / 1000} seconds)");
				BaselineTimer.Start();
				foreach(var wellTest in NadhBaselineWells)
				{
					StatusMessages.Add($"Dispensing {Config.NadhVolume}µg NADH to {wellTest.Well.Label}");
					Engine.QueueCommands(new CommandPacket(
					[
						new AlignWellToAxis(Axis.Nadh1, wellTest.Well, Device),
						new EmitFluid(Axis.Nadh1, Config.NadhVolume, Device),
					]));
				}

				StatusMessages.Add($"Reading NADH baselines...");
				SendReadingCommand(NadhBaselineWells[0].Well, Config.DnaLaserLocation, "Low NADH", new ReadNadh(Device), NadhLaserReadingKey);
				if (_processTerminating)
					return;

				SendReadingCommand(NadhBaselineWells[1].Well, Config.DnaLaserLocation, "High NADH", new ReadNadh(Device), NadhLaserReadingKey);
				if (_processTerminating)
					return;

				StatusMessages.Add($"Starting DNA baseline timer ({(DateTime.Now - baselineTimerStarted).Seconds} seconds remaining)");
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
                SendReadingCommand(DnaBaselineWells[0].Well, Config.DnaLaserLocation, "1 µg DNA", new ReadDna(Device), nameof(Microgram1Dna));
				if (_processTerminating)
					return;

                SendReadingCommand(DnaBaselineWells[1].Well, Config.DnaLaserLocation, "0.25 µg DNA", new ReadDna(Device), nameof(MicrogramDna25Dna));
				if (_processTerminating)
					return;

                SendReadingCommand(DnaBaselineWells[2].Well, Config.DnaLaserLocation, "0.0625 µg DNA", new ReadDna(Device), nameof(MicrogramDna0625Dna));
				if (_processTerminating)
					return;

                SendReadingCommand(DnaBaselineWells[3].Well, Config.DnaLaserLocation, "0 µg DNA", new ReadDna(Device), nameof(Microgram0Dna));
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

		private void SendReadingCommand(Well well, Coordinate location, string readingName, Command readCommand, string tokenKey)
		{
			readCommand.ResultDataToken = new KeyValuePair<string, string>($"{well.Label}:{readingName}", tokenKey);
			StatusMessages.Add($"Reading {readingName} for {well.Label}");
			var result = Engine.QueueCommandsForResult(new CommandPacket(
			[
				new AlignWellToLocation(location, well, Device),
				readCommand
			]));
		}

		private const string DnaLaserReadingKey = "1.io6.in";
		private const string NadhLaserReadingKey = "2.io6.in";
		private const string HighNadhName = "HighNadh";
		private const string LowNadhName = "LowNadh";
		private void Engine_DataReceived(object sender, DataArgs<KeyValuePair<string, string>> e)
		{
			var recievedValue = e.Data.Value;
			var readingTitle = e.Data.Key;
			var afterReading = recievedValue.Split("reading ")[1];
			var firstNumber = afterReading.Split(',')[0];
			if(float.TryParse(firstNumber, out var readingValue))
			{
				if (readingTitle.EndsWith(HighNadhName))
				{
					HighNadh = readingValue;
				}
				else if (readingTitle.EndsWith(LowNadhName))
				{
					LowNadh = readingValue;
				}
				else
				{
					var testWell = TestWells.FirstOrDefault(w => w.Label.StartsWith(readingTitle));
					testWell.TimeValues.Add(Int32.Parse(readingTitle.Split(':')[1]), readingValue);
				}
				StatusMessages.Add($"{readingTitle} is {readingValue}");
			}
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
        private void NadhMediaTimer_Elapsed(object sender, DataArgs<int> e)
        {
            if (_processTerminating)
                return;

            _nadhTimerLock.Wait();

            if (_processTerminating)
                return;

            try
            {
                var testWell = sender as TestWell;
                TestInKohPhase?.Invoke(this, new DataArgs<TestWell>(testWell));
                StatusMessages.Add($"Adding NADH to {testWell.Label}");
                Axis nadhAxis = testWell.Well.Plate.Index.X == 0 ? Axis.Nadh1 : Axis.Nadh2;
                var packet1 = new CommandPacket();
                // remove fluid from well
                packet1.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.TestMediaAspirationVolume));
                packet1.Commands.AddRange(
                [
					// add NADH to well
                    new AlignWellToAxis(nadhAxis, testWell.Well, Device),
                    new EmitFluid(nadhAxis, Config.NadhVolume, Device),
                ]);
                Engine.QueueCommands(packet1);
				SendReadingCommand(testWell.Well, Config.DnaLaserLocation, $"NADH {e.Data}", new ReadNadh(Device), $"TimeValue{e.Data}");
                if (_processTerminating)
                    return;
            }
            catch (Exception ex)
            {
                Terminate($"Error preparing test media: {ex.Message}");
            }
            finally
            {
                _nadhTimerLock.Release();
            }
        }
        private void TestMediaTimer_Elapsed(object sender, DataArgs<int> e)
		{
			if (_processTerminating)
				return;

			_testMediaTimerLock.Wait();

			if (_processTerminating)
				return;

			try
			{
				var testWell = sender as TestWell;
				switch (e.Data)
				{
					case 1:
                        SendReadingCommand(testWell.Well, Config.DnaLaserLocation, $"NADH {e.Data}", new ReadNadh(Device), $"TimeValue{e.Data}");
                        if (_processTerminating)
                            return;
                        break;
                    case 2:
                        SendReadingCommand(testWell.Well, Config.DnaLaserLocation, $"NADH {e.Data}", new ReadNadh(Device), $"TimeValue{e.Data}");
                        if (_processTerminating)
                            return;
                        break;
                    case 3:
                        SendReadingCommand(testWell.Well, Config.DnaLaserLocation, $"NADH {e.Data}", new ReadNadh(Device), $"TimeValue{e.Data}");
                        if (_processTerminating)
                            return;
                        break;
                    case 4:
                        SendReadingCommand(testWell.Well, Config.DnaLaserLocation, $"NADH {e.Data}", new ReadNadh(Device), $"TimeValue{e.Data}");
                        if (_processTerminating)
                            return;
                        StatusMessages.Add($"Adding KOH media to {testWell.Label}");
						Axis kohAxis = testWell.Well.Plate.Index.X == 0 ? Axis.Koh1 : Axis.Koh2;
                        var packet2 = new CommandPacket();
                        // remove fluid from well
                        packet2.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.KohAspirationVolume));
                        packet2.Commands.AddRange(
                        [
							// add KOH to well
							new AlignWellToAxis(kohAxis, testWell.Well, Device),
                            new EmitFluid(kohAxis, Config.KohVolume, Device),
                        ]);
                        Engine.QueueCommands(packet2);
						testWell.KohTimer.Start();
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
		private void KohTimer_Elapsed(object sender, DataArgs<int> e)
		{
			_kohTimerLock.Wait();
			try
			{
				var testWell = sender as TestWell;
				TestInDnaPhase?.Invoke(this, new DataArgs<TestWell>(testWell));
				StatusMessages.Add($"Adding DNA assay media to {testWell.Label}");
				Axis dnaAxis = testWell.Well.Plate.Index.X == 0 ? Axis.DnaReagent1 : Axis.DnaReagent2;
                var packet = new CommandPacket();
                // remove KOH from well
                packet.Commands.AddRange(GetAspirationCommands(FluidType.Koh, testWell, Config.DnaAspirationVolume));
                packet.Commands.AddRange(
                [
					// add DNA reagent to well
					new AlignWellToAxis(dnaAxis, testWell.Well, Device),
                    new EmitFluid(dnaAxis, Config.DnaVolume, Device),
                ]);
                Engine.QueueCommands(packet);
				testWell.DnaTimer.Start();
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
		private void DnaTimer_Elapsed(object sender, DataArgs<int> e)
		{
			_dnaTimerLock.Wait();
			try
			{
				var testWell = sender as TestWell;
				SendReadingCommand(testWell.Well, Config.DnaLaserLocation, $"DNA", new ReadNadh(Device), $"TimeValue{e.Data}");
				if (_processTerminating)
					return;
                //// remove remaining fluid from well
                //var packet = new CommandPacket();
                //// remove KOH from well
                //packet.Commands.AddRange(GetAspirationCommands(FluidType.General, testWell, Config.CleanUpAspirationVolume));
                //Engine.QueueCommands(packet);

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
            //var axis = fluidType switch
            //{
            //    FluidType.General => Axis.AspirateFluid,
            //    FluidType.Koh => Axis.AspirateKoh
            //};
            var axis = Axis.AspirateKoh;
            //Command removeCommand = fluidType switch
            //{
            //    FluidType.General => new RemoveFluid(Config.CleanUpAspirationVolume, Device),
            //    FluidType.Koh => new RemoveKoh(Config.CleanUpAspirationVolume, Device),
            //    _ => null
            //};
            Command removeCommand = new RemoveKoh(volume, Device);
            //Command emptyCommand = fluidType switch
            //{
            //    FluidType.General => new EmptyAspirateFluid(Device),
            //    FluidType.Koh => new EmptyAspirateKoh(Device),
            //    _ => null
            //};
            Command emptyCommand = new EmptyAspirateKoh(Device);
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

		public List<TestWell> DnaBaselineWells { get; private set; }
		public List<TestWell> NadhBaselineWells { get; private set; }

		public List<TestWell> AllTestWells { get; private set; } = new();
		public List<TestWell> TestWells1 { get; private set; }
		public List<TestWell> TestWells2 { get; private set; }
		public System.Timers.Timer BaselineTimer { get; set; }
		public ObservableCollection<string> StatusMessages { get; } = new();
		public DeviceControl Device { get; private set; }
		public ProcessEngine Engine { get; private set; }
		public DeviceConfiguration Config => Device.Configuration;

		public event EventHandler PreparationComplete;
		public event EventHandler Terminated;
		public event EventHandler<DataArgs<TestWell>> TestInKohPhase;
		public event EventHandler<DataArgs<TestWell>> TestInDnaPhase;
		public event EventHandler<DataArgs<TestWell>> TestComplete;

		int _baselineWellCount => DnaBaselineWells.Count() + NadhBaselineWells.Count();
		int _testWellCount => TestWells.Count - _baselineWellCount;
		bool _doingPrepToBaselineTimerStart;
		bool _processTerminating;
        SemaphoreSlim _nadhTimerLock = new SemaphoreSlim(1, 1);
        SemaphoreSlim _testMediaTimerLock = new SemaphoreSlim(1, 1);
		public SemaphoreSlim _kohTimerLock = new SemaphoreSlim(1, 1);
		SemaphoreSlim _dnaTimerLock = new SemaphoreSlim(1, 1);
	}
}
