using DevExpress.Mvvm;
using Effani.Device.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Effani.Device.Control.DeviceControl;

namespace Effani.Device.UI
{
	public class TestWell : ViewModelBase, IDisposable
	{
		#region Constructors
		public TestWell(DeviceConfiguration deviceConfiguration, Well well, int testIndex, EventHandler<DataArgs<int>> nadhTimerCallback, EventHandler<DataArgs<int>> testMediaTimerCallback, EventHandler<DataArgs<int>> kohTimerCallback, EventHandler<DataArgs<int>> dnaTimerCallback)
        {
            _nadhTimerCallback = nadhTimerCallback;
            _dnaTimerCallback = dnaTimerCallback;
			_kohTimerCallback = kohTimerCallback;
			_testMediaTimerCallback = testMediaTimerCallback;
			TestIndex = testIndex;
			Well = well;
            NadhTimer1h = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.TestMediaTimerPeriod)) { AutoReset = false };
            TestMediaTimer1h = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.TestMediaTimerPeriod)) { AutoReset = false };
			TestMediaTimer2h = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.TestMediaTimerPeriod * 2)) { AutoReset = false };
			TestMediaTimer3h = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.TestMediaTimerPeriod * 3)) { AutoReset = false };
			TestMediaTimer4h = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.TestMediaTimerPeriod * 4)) { AutoReset = false };
			KohTimer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.KohTimerPeriod)) { AutoReset = false };
			DnaTimer = new Timer(TimeSpan.FromMilliseconds(deviceConfiguration.DnaTimerPeriod)) { AutoReset = false };

            NadhTimer1h.Elapsed += NadahTimer1h_Elapsed;
            TestMediaTimer1h.Elapsed += TestMediaTimer1h_Elapsed;
			TestMediaTimer2h.Elapsed += TestMediaTimer2h_Elapsed;
			TestMediaTimer3h.Elapsed += TestMediaTimer3h_Elapsed;
			TestMediaTimer4h.Elapsed += TestMediaTimer4h_Elapsed;
			KohTimer.Elapsed += KohTimer_Elapsed;
			DnaTimer.Elapsed += DnaTimer_Elapsed;
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			TestMediaTimer1h.Stop();
			TestMediaTimer2h.Stop();
			TestMediaTimer3h.Stop();
			TestMediaTimer4h.Stop();
			KohTimer.Stop();
			DnaTimer.Stop();

            NadhTimer1h.Elapsed -= NadahTimer1h_Elapsed;
            TestMediaTimer1h.Elapsed -= TestMediaTimer1h_Elapsed;
			TestMediaTimer2h.Elapsed -= TestMediaTimer2h_Elapsed;
			TestMediaTimer3h.Elapsed -= TestMediaTimer3h_Elapsed;
			TestMediaTimer4h.Elapsed -= TestMediaTimer4h_Elapsed;
			KohTimer.Elapsed -= KohTimer_Elapsed;
			DnaTimer.Elapsed -= DnaTimer_Elapsed;
		}
		#endregion

		#region Private methods
        private void NadahTimer1h_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(() => _nadhTimerCallback.Invoke(this, new DataArgs<int>(1)));
        }
        private void TestMediaTimer1h_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _testMediaTimerCallback.Invoke(this, new DataArgs<int>(2)));
		}
		private void TestMediaTimer2h_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _testMediaTimerCallback.Invoke(this, new DataArgs<int>(3)));
		}
		private void TestMediaTimer3h_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _testMediaTimerCallback.Invoke(this, new DataArgs<int>(4)));
		}
		private void TestMediaTimer4h_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _testMediaTimerCallback.Invoke(this, new DataArgs<int>(5)));
		}
		private void KohTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _kohTimerCallback.Invoke(this, new DataArgs<int>(6)));
        }
		private void DnaTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _dnaTimerCallback.Invoke(this, new DataArgs<int>(7)));
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
		public Timer DnaTimer { get; set; }
		public float DnaValue { get; set; }
		public Timer KohTimer { get; set; }
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
        public Timer NadhTimer1h { get; set; }
        public Timer TestMediaTimer1h { get; set; }
		public Timer TestMediaTimer2h { get; set; }
		public Timer TestMediaTimer3h { get; set; }
		public Timer TestMediaTimer4h { get; set; }
		public Well Well { get; private set; }
		public int TestIndex { get; }
		#endregion

		#region Fields
		bool _canceled = false;
        readonly EventHandler<DataArgs<int>> _nadhTimerCallback;
        readonly EventHandler<DataArgs<int>> _dnaTimerCallback;
		readonly EventHandler<DataArgs<int>> _kohTimerCallback;
		bool _loaded = false;
		string _sampleId;
		readonly EventHandler<DataArgs<int>> _testMediaTimerCallback;
		public Dictionary<int, float> TimeValues = new();

		#endregion
	}
}
