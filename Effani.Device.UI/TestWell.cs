using DevExpress.Mvvm;
using Effani.Device.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Effani.Device.Control.AbalDeviceControl;

namespace Effani.Device.UI
{
	public class TestWell : ViewModelBase, IDisposable
	{
		#region Constructors
		public TestWell(DeviceConfiguration deviceConfiguration, Well well, int testIndex, EventHandler<DataArgs<int>> basalIncubationTimerCallback, EventHandler<DataArgs<int>> nadhReadingsTimerCallback, EventHandler<DataArgs<int>> kohDwellTimerCallback, EventHandler<DataArgs<int>> dnaIncubationTimerCallback)
        {
            _basalIncubationTimerCallback = basalIncubationTimerCallback;
            _dnaIncubationTimerCallback = dnaIncubationTimerCallback;
			_kohDwellDnaTimerCallback = kohDwellTimerCallback;
			_nadhReadingsTimerCallback = nadhReadingsTimerCallback;
			TestIndex = testIndex;
			Well = well;
            BasalIncubationTimer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.NadhReadingsTimerPeriod)) { AutoReset = false };
            NadhReadings2Timer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.NadhReadingsTimerPeriod)) { AutoReset = false };
			NadhReadings3Timer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.NadhReadingsTimerPeriod * 2)) { AutoReset = false };
			NadhReadings4Timer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.NadhReadingsTimerPeriod * 3)) { AutoReset = false };
			NadhReadings5Timer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.NadhReadingsTimerPeriod * 4)) { AutoReset = false };
			KohDwellTimer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.KohTimerPeriod)) { AutoReset = false };
			DnaIncubationTimer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.DnaTimerPeriod)) { AutoReset = false };

            BasalIncubationTimer.Elapsed += BasalIncubationTimer_Elapsed;
            NadhReadings2Timer.Elapsed += NadhReadings2Timer_Elapsed;
			NadhReadings3Timer.Elapsed += NadhReadings3Timer_Elapsed;
			NadhReadings4Timer.Elapsed += NadhReadings4Timer_Elapsed;
			NadhReadings5Timer.Elapsed += NadhReadings5Timer_Elapsed;
			KohDwellTimer.Elapsed += KohDwellTimer_Elapsed;
			DnaIncubationTimer.Elapsed += DnaIncubationTimer_Elapsed;
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			NadhReadings2Timer.Stop();
			NadhReadings3Timer.Stop();
			NadhReadings4Timer.Stop();
			NadhReadings5Timer.Stop();
			KohDwellTimer.Stop();
			DnaIncubationTimer.Stop();

            BasalIncubationTimer.Elapsed -= BasalIncubationTimer_Elapsed;
            NadhReadings2Timer.Elapsed -= NadhReadings2Timer_Elapsed;
			NadhReadings3Timer.Elapsed -= NadhReadings3Timer_Elapsed;
			NadhReadings4Timer.Elapsed -= NadhReadings4Timer_Elapsed;
			NadhReadings5Timer.Elapsed -= NadhReadings5Timer_Elapsed;
			KohDwellTimer.Elapsed -= KohDwellTimer_Elapsed;
			DnaIncubationTimer.Elapsed -= DnaIncubationTimer_Elapsed;
		}
		#endregion

		#region Private methods
        private void BasalIncubationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(() => _basalIncubationTimerCallback.Invoke(this, new DataArgs<int>(1)));
        }
        private void NadhReadings2Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(2)));
		}
		private void NadhReadings3Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(3)));
		}
		private void NadhReadings4Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(4)));
		}
		private void NadhReadings5Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(5)));
		}
		private void KohDwellTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _kohDwellDnaTimerCallback.Invoke(this, new DataArgs<int>(6)));
        }
		private void DnaIncubationTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _dnaIncubationTimerCallback.Invoke(this, new DataArgs<int>(7)));
		}
		#endregion

		#region Properties
        public bool Canceled
		{
			get
			{
				return _canceled;
			}
			set
			{
				if (_canceled == value)
					return;
				_canceled = value;
				RaisePropertyChanged(nameof(Canceled));
			}
		}
		public Timer DnaIncubationTimer { get; set; }
		public float DnaValue { get; set; }
		public Timer KohDwellTimer { get; set; }
		public string Label => Well.Label;
		public bool LoadedOntoTray
		{
			get
			{
				return _loaded;
			}
			set
			{
				if (_loaded == value)
					return;
				_loaded = value;
				RaisePropertyChanged(nameof(LoadedOntoTray));
			}
		}
		public string SampleId
		{
			get
			{
				return _sampleId;
			}
			set
			{
				if (_sampleId == value)
					return;
				_sampleId = value;
				RaisePropertyChanged(nameof(SampleId));
			}
        }
        public Timer BasalIncubationTimer { get; set; }
        public Timer NadhReadings2Timer { get; set; }
		public Timer NadhReadings3Timer { get; set; }
		public Timer NadhReadings4Timer { get; set; }
		public Timer NadhReadings5Timer { get; set; }
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
		public Well Well { get; private set; }
		public int TestIndex { get; }
		#endregion

		#region Fields
		bool _canceled = false;
        readonly EventHandler<DataArgs<int>> _basalIncubationTimerCallback;
        readonly EventHandler<DataArgs<int>> _dnaIncubationTimerCallback;
		readonly EventHandler<DataArgs<int>> _kohDwellDnaTimerCallback;
		bool _loaded = false;
		string _sampleId;
		readonly EventHandler<DataArgs<int>> _nadhReadingsTimerCallback;
		public Dictionary<int, float> TimeValues = new();

		#endregion
	}
}
