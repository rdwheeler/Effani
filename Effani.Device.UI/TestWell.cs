using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Effani.Device.Control;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Effani.Device.Control.DeviceControl;

namespace Effani.Device.UI
{
	public class TestWell : ViewModelBase, IDisposable
	{
		public TestWell(DeviceConfiguration deviceConfiguration, Well well, EventHandler<DataArgs<int>> testMediaTimerCallback, EventHandler<DataArgs<int>> kohTimerCallback, EventHandler<DataArgs<int>> dnaTimerCallback)
		{
			_dnaTimerCallback = dnaTimerCallback;
			_kohTimerCallback = kohTimerCallback;
			_testMediaTimerCallback = testMediaTimerCallback;
			Well = well;
			TestMediaTimer1h = new Timer(deviceConfiguration.TestMediaTimerPeriod) { AutoReset = false };
			TestMediaTimer2h = new Timer(deviceConfiguration.TestMediaTimerPeriod * 2) { AutoReset = false };
			TestMediaTimer3h = new Timer(deviceConfiguration.TestMediaTimerPeriod * 3) { AutoReset = false };
			TestMediaTimer4h = new Timer(deviceConfiguration.TestMediaTimerPeriod * 4) { AutoReset = false };
			KohTimer = new Timer(deviceConfiguration.KohTimerPeriod) { AutoReset = false };
			DnaTimer = new Timer(deviceConfiguration.DnaTimerPeriod) { AutoReset = false };

			TestMediaTimer1h.Elapsed += TestMediaTimer1h_Elapsed;
			TestMediaTimer2h.Elapsed += TestMediaTimer2h_Elapsed;
			TestMediaTimer3h.Elapsed += TestMediaTimer3h_Elapsed;
			TestMediaTimer4h.Elapsed += TestMediaTimer4h_Elapsed;
			KohTimer.Elapsed += KohTimer_Elapsed;
			DnaTimer.Elapsed += DnaTimer_Elapsed;
		}

		private void DnaTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _dnaTimerCallback.Invoke(this, new DataArgs<int>(0)));
		}

		private void KohTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _kohTimerCallback.Invoke(this, new DataArgs<int>(0)));
		}

		private void TestMediaTimer4h_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _testMediaTimerCallback.Invoke(this, new DataArgs<int>(4)));
		}

		private void TestMediaTimer3h_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _testMediaTimerCallback.Invoke(this, new DataArgs<int>(3)));
		}

		private void TestMediaTimer2h_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _testMediaTimerCallback.Invoke(this, new DataArgs<int>(2)));
		}

		private void TestMediaTimer1h_Elapsed(object sender, ElapsedEventArgs e)
		{
			Task.Run(() => _testMediaTimerCallback.Invoke(this, new DataArgs<int>(1)));
		}

		public void Dispose()
		{
			TestMediaTimer1h.Stop();
			TestMediaTimer2h.Stop();
			TestMediaTimer3h.Stop();
			TestMediaTimer4h.Stop();
			KohTimer.Stop();
			DnaTimer.Stop();

			TestMediaTimer1h.Elapsed -= TestMediaTimer1h_Elapsed;
			TestMediaTimer2h.Elapsed -= TestMediaTimer2h_Elapsed;
			TestMediaTimer3h.Elapsed -= TestMediaTimer3h_Elapsed;
			TestMediaTimer4h.Elapsed -= TestMediaTimer4h_Elapsed;
			KohTimer.Elapsed -= KohTimer_Elapsed;
			DnaTimer.Elapsed -= DnaTimer_Elapsed;
		}

		public float Time1Value { get; set; }
		public float Time2Value { get; set; }
		public float Time3Value { get; set; }
		public float Time4Value { get; set; }
		public float DnaValue { get; set; }
		bool _canceled = false;
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
		bool _loaded = false;
		public bool Loaded
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
				RaisePropertyChanged(nameof(Loaded));
			}
		}
		public Well Well { get; private set; }
		public string Label => Well.Label;
		string _sampleId;
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

		public Timer TestMediaTimer1h { get; set; }

		public Timer TestMediaTimer2h { get; set; }
		public Timer TestMediaTimer3h { get; set; }
		public Timer TestMediaTimer4h { get; set; }
		public Timer KohTimer { get; set; }
		public Timer DnaTimer { get; set; }

		readonly EventHandler<DataArgs<int>> _testMediaTimerCallback;
		readonly EventHandler<DataArgs<int>> _kohTimerCallback;
		readonly EventHandler<DataArgs<int>> _dnaTimerCallback;
	}
}
