
using DevExpress.Mvvm;
using System;
using System.Windows;

namespace Effani.Device.DataCollection
{
	public class TestWellViewModel : ViewModelBase, IDisposable
	{
		#region Constructors
		public TestWellViewModel(TestWell testWell)
		{
			TestWell = testWell;
			testWell.PropertyChanged += TestWell_PropertyChanged;
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			TestWell.PropertyChanged -= TestWell_PropertyChanged;
		}
		#endregion

		#region Private methods
		private void TestWell_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TestWell.CurrentTimerDescription))
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					RaisePropertyChanged(nameof(CurrentTimerDescription));
				});
			}
		}
		#endregion

		#region Properties
		public string CurrentTimerDescription => TestWell.CurrentTimerDescription;
		public float DnaValue { get; set; }
		public string Label => TestWell.Label;
		public string SampleId => TestWell.SampleId;
		public TestWell TestWell { get; set; }
		#endregion
	}
}
