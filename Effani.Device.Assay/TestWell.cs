using Drivetrain.Robotics.Control;
using System;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Effani.Device.Assay
{
	public class TestWell : IDisposable, INotifyPropertyChanged
	{
		#region Constructors
		public TestWell() { }
		#endregion

		#region Constructors
		public TestWell(DeviceConfiguration deviceConfiguration, Well well, int testIndex, EventHandler<DataArgs<int>> basalIncubationTimerCallback, EventHandler<DataArgs<int>> nadhReadingsTimerCallback, EventHandler<DataArgs<int>> kohDwellTimerCallback, EventHandler<DataArgs<int>> dnaIncubationTimerCallback)
		{
			_basalIncubationTimerCallback = basalIncubationTimerCallback;
			_dnaIncubationTimerCallback = dnaIncubationTimerCallback;
			_kohDwellDnaTimerCallback = kohDwellTimerCallback;
			_nadhReadingsTimerCallback = nadhReadingsTimerCallback;
			_timerDescriptionUpdateInterval = deviceConfiguration.TimerDescriptionUpdateInterval;
			TestIndex = testIndex;
			Well = well;

			BasalIncubationCount = deviceConfiguration.BasalIncubationTimerPeriod / _timerDescriptionUpdateInterval;
			NadhReadings1Count = deviceConfiguration.NadhReading1TimerPeriod / _timerDescriptionUpdateInterval;
			NadhReadings2Count = deviceConfiguration.NadhReadingsTimerPeriod / _timerDescriptionUpdateInterval;
			NadhReadings3Count = deviceConfiguration.NadhReadingsTimerPeriod / _timerDescriptionUpdateInterval;
			NadhReadings4Count = deviceConfiguration.NadhReadingsTimerPeriod / _timerDescriptionUpdateInterval;
			NadhReadings5Count = deviceConfiguration.NadhReadingsTimerPeriod / _timerDescriptionUpdateInterval;
			KohDwellCount = deviceConfiguration.KohTimerPeriod / _timerDescriptionUpdateInterval;
			DnaIncubationCount = deviceConfiguration.DnaTimerPeriod / _timerDescriptionUpdateInterval;
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			if (BasalIncubationTimer is not null)
				BasalIncubationTimer.Elapsed -= BasalIncubationTimer_Elapsed;
			if (NadhReadings1Timer is not null)
				NadhReadings1Timer.Elapsed -= NadhReadings1Timer_Elapsed;
			if (NadhReadings2Timer is not null)
				NadhReadings2Timer.Elapsed -= NadhReadings2Timer_Elapsed;
			if (NadhReadings3Timer is not null)
				NadhReadings3Timer.Elapsed -= NadhReadings3Timer_Elapsed;
			if (NadhReadings4Timer is not null)
				NadhReadings4Timer.Elapsed -= NadhReadings4Timer_Elapsed;
			if (NadhReadings5Timer is not null)
				NadhReadings5Timer.Elapsed -= NadhReadings5Timer_Elapsed;
			if (KohDwellTimer is not null)
				KohDwellTimer.Elapsed -= KohDwellTimer_Elapsed;
			if (DnaIncubationTimer is not null)
				DnaIncubationTimer.Elapsed -= DnaIncubationTimer_Elapsed;

			BasalIncubationTimer = DisposeTimer(BasalIncubationTimer);
			NadhReadings1Timer = DisposeTimer(NadhReadings1Timer);
			NadhReadings2Timer = DisposeTimer(NadhReadings2Timer);
			NadhReadings3Timer = DisposeTimer(NadhReadings3Timer);
			NadhReadings4Timer = DisposeTimer(NadhReadings4Timer);
			NadhReadings5Timer = DisposeTimer(NadhReadings5Timer);
			KohDwellTimer = DisposeTimer(KohDwellTimer);
			KohDwellTimer = DisposeTimer(KohDwellTimer);
			DnaIncubationTimer = DisposeTimer(DnaIncubationTimer);
		}
		public void InitializeTimers()
		{
			BasalIncubationTimer = new Timer(TimeSpan.FromMilliseconds(_timerDescriptionUpdateInterval)) { AutoReset = false };
			NadhReadings1Timer = new Timer(TimeSpan.FromMilliseconds(_timerDescriptionUpdateInterval)) { AutoReset = false };
			NadhReadings2Timer = new Timer(TimeSpan.FromMilliseconds(_timerDescriptionUpdateInterval)) { AutoReset = false };
			NadhReadings3Timer = new Timer(TimeSpan.FromMilliseconds(_timerDescriptionUpdateInterval)) { AutoReset = false };
			NadhReadings4Timer = new Timer(TimeSpan.FromMilliseconds(_timerDescriptionUpdateInterval)) { AutoReset = false };
			NadhReadings5Timer = new Timer(TimeSpan.FromMilliseconds(_timerDescriptionUpdateInterval)) { AutoReset = false };
			KohDwellTimer = new Timer(TimeSpan.FromMilliseconds(_timerDescriptionUpdateInterval)) { AutoReset = false };
			DnaIncubationTimer = new Timer(TimeSpan.FromMilliseconds(_timerDescriptionUpdateInterval)) { AutoReset = false };

			SetCurrentTimerDescription(BasalIncubationCount);

			BasalIncubationTimer.Elapsed += BasalIncubationTimer_Elapsed;
			NadhReadings1Timer.Elapsed += NadhReadings1Timer_Elapsed;
			NadhReadings2Timer.Elapsed += NadhReadings2Timer_Elapsed;
			NadhReadings3Timer.Elapsed += NadhReadings3Timer_Elapsed;
			NadhReadings4Timer.Elapsed += NadhReadings4Timer_Elapsed;
			NadhReadings5Timer.Elapsed += NadhReadings5Timer_Elapsed;
			KohDwellTimer.Elapsed += KohDwellTimer_Elapsed;
			DnaIncubationTimer.Elapsed += DnaIncubationTimer_Elapsed;
		}
		#endregion

		#region Private methods
		private void SetCurrentTimerDescription(int count, string prefix = null)
		{
			var timeString = count == 0 ? null : TimeSpan.FromMilliseconds(count * _timerDescriptionUpdateInterval).ToString("c");
			CurrentTimerDescription = $"{prefix}{timeString}";
		}
		protected void RaisePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private Timer DisposeTimer(Timer timer)
		{
			if (timer is not null)
			{
				timer.Stop();
				timer.Dispose();
			}

			return null;
		}

		private void BasalIncubationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
			if(BasalIncubationTimer is null)
				return;

			BasalIncubationCount--;
			if (BasalIncubationCount <= 0)
			{
				SetCurrentTimerDescription(0, "1@waiting");
				BasalIncubationTimer.Elapsed -= BasalIncubationTimer_Elapsed;
				BasalIncubationTimer = DisposeTimer(BasalIncubationTimer);
				Task.Run(() => _basalIncubationTimerCallback.Invoke(this, new DataArgs<int>(0)));
			}
			else
			{
				SetCurrentTimerDescription(BasalIncubationCount);
				BasalIncubationTimer.Start();
			}
		}

		private void NadhReadings1Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (NadhReadings1Timer is null)
				return;

			NadhReadings1Count--;
			if (NadhReadings1Count <= 0)
			{
				SetCurrentTimerDescription(0, "2@waiting");
				NadhReadings1Timer.Elapsed -= NadhReadings1Timer_Elapsed;
				NadhReadings1Timer = DisposeTimer(NadhReadings1Timer);
				Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(1)));
			}
			else
			{
				SetCurrentTimerDescription(NadhReadings1Count);
				NadhReadings1Timer.Start();
			}
		}
		private void NadhReadings2Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (NadhReadings2Timer is null)
				return;

			NadhReadings2Count--;
			if (NadhReadings2Count <= 0)
			{
				SetCurrentTimerDescription(0, "3@waiting");
				NadhReadings2Timer.Elapsed -= NadhReadings2Timer_Elapsed;
				NadhReadings2Timer = DisposeTimer(NadhReadings2Timer);
				Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(2)));
			}
			else
			{
				SetCurrentTimerDescription(NadhReadings2Count, "2@");
				NadhReadings2Timer.Start();
			}
		}
		private void NadhReadings3Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (NadhReadings3Timer is null)
				return;

			NadhReadings3Count--;
			if (NadhReadings3Count <= 0)
			{
				SetCurrentTimerDescription(0, "4@waiting");
				NadhReadings3Timer.Elapsed -= NadhReadings3Timer_Elapsed;
				NadhReadings3Timer = DisposeTimer(NadhReadings3Timer);
				Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(3)));
			}
			else
			{
				SetCurrentTimerDescription(NadhReadings3Count, "3@");
				NadhReadings3Timer.Start();
			}
		}
		private void NadhReadings4Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (NadhReadings4Timer is null)
				return;

			NadhReadings4Count--;
			if (NadhReadings4Count <= 0)
			{
				SetCurrentTimerDescription(0, "5@waiting");
				NadhReadings4Timer.Elapsed -= NadhReadings4Timer_Elapsed;
				NadhReadings4Timer = DisposeTimer(NadhReadings4Timer);
				Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(4)));
			}
			else
			{
				SetCurrentTimerDescription(NadhReadings4Count, "4@");
				NadhReadings4Timer.Start();
			}
		}
		private void NadhReadings5Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (NadhReadings5Timer is null)
				return;

			NadhReadings5Count--;
			if (NadhReadings5Count <= 0)
			{
				SetCurrentTimerDescription(0, "waiting");
				NadhReadings5Timer.Elapsed -= NadhReadings5Timer_Elapsed;
				NadhReadings5Timer = DisposeTimer(NadhReadings5Timer);
				Task.Run(() => _nadhReadingsTimerCallback.Invoke(this, new DataArgs<int>(5)));
			}
			else
			{
				SetCurrentTimerDescription(NadhReadings5Count, "5@");
				NadhReadings5Timer.Start();
			}
		}
		private void KohDwellTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (KohDwellTimer is null)
				return;

			KohDwellCount--;
			if (KohDwellCount <= 0)
			{
				SetCurrentTimerDescription(0, "waiting");
				KohDwellTimer.Elapsed -= KohDwellTimer_Elapsed;
				KohDwellTimer = DisposeTimer(KohDwellTimer);
				Task.Run(() => _kohDwellDnaTimerCallback.Invoke(this, new DataArgs<int>(6)));
			}
			else
			{
				SetCurrentTimerDescription(KohDwellCount);
				KohDwellTimer.Start();
			}
		}
		private void DnaIncubationTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (DnaIncubationTimer is null)
				return;

			DnaIncubationCount--;
			if (DnaIncubationCount <= 0)
			{
				SetCurrentTimerDescription(0);
				DnaIncubationTimer.Elapsed -= DnaIncubationTimer_Elapsed;
				DnaIncubationTimer = DisposeTimer(DnaIncubationTimer);
				Task.Run(() => _dnaIncubationTimerCallback.Invoke(this, new DataArgs<int>(7)));
			}
			else
			{
				SetCurrentTimerDescription(DnaIncubationCount);
				DnaIncubationTimer.Start();
			}
		}
		#endregion

		#region Properties
		#region CurrentTimerDescription
		public string CurrentTimerDescription
		{
			get
			{
				return _currentTimerDescription;
			}
			set
			{
				if (_currentTimerDescription == value)
					return;
				_currentTimerDescription = value;
				RaisePropertyChanged(nameof(CurrentTimerDescription));
			}
		}
		string _currentTimerDescription;
		#endregion
		#region Canceled
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
		bool _canceled = false;
		#endregion
		#region SampleId
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
		string _sampleId;
		#endregion
		public float DnaValue { get; set; }
		public string Label => Well.Label;
		public Timer BasalIncubationTimer { get; set; }
		public Timer NadhReadings1Timer { get; set; }
		public Timer NadhReadings2Timer { get; set; }
		public Timer NadhReadings3Timer { get; set; }
		public Timer NadhReadings4Timer { get; set; }
		public Timer NadhReadings5Timer { get; set; }
		public Timer KohDwellTimer { get; set; }
		public Timer DnaIncubationTimer { get; set; }
		public Well Well { get; private set; }
		public int TestIndex { get; }
		public Dictionary<int, float> TimeValues = new();
		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		#region Fields
		readonly EventHandler<DataArgs<int>> _basalIncubationTimerCallback;
		readonly EventHandler<DataArgs<int>> _dnaIncubationTimerCallback;
		readonly EventHandler<DataArgs<int>> _kohDwellDnaTimerCallback;
		readonly EventHandler<DataArgs<int>> _nadhReadingsTimerCallback;
		private int BasalIncubationCount;
		private int NadhReadings1Count;
		private int NadhReadings2Count;
		private int NadhReadings3Count;
		private int NadhReadings4Count;
		private int NadhReadings5Count;
		private int KohDwellCount;
		private int DnaIncubationCount;
		int _timerDescriptionUpdateInterval;
		#endregion
	}
}
