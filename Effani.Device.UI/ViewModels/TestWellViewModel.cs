
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
					RaisePropertyChanged(nameof(StageProgress));
					RaisePropertyChanged(nameof(StageComplete));
					RaisePropertyChanged(nameof(StageInProgress));
					RaisePropertyChanged(nameof(StageReading));
					RaisePropertyChanged(nameof(StageTimeRemaining));
					RaisePropertyChanged(nameof(StageStatusText));
				});
			}
		}
		#endregion

		#region Properties
		public string CurrentTimerDescription => TestWell.CurrentTimerDescription;
		/// <summary>
		/// The NADH reading number from a "N@" prefix on the timer description, or null when there is none.
		/// </summary>
		public string StageReading
		{
			get
			{
				var description = CurrentTimerDescription;
				var at = description?.IndexOf('@') ?? -1;
				return at > 0 ? description.Substring(0, at) : null;
			}
		}
		/// <summary>
		/// The timer description without its "N@" prefix: either the time remaining or "waiting".
		/// </summary>
		public string StageTimeRemaining
		{
			get
			{
				var description = CurrentTimerDescription;
				var at = description?.IndexOf('@') ?? -1;
				return at >= 0 ? description.Substring(at + 1) : description;
			}
		}
		public bool StageComplete => StageTimeRemaining == "waiting";
		public bool StageInProgress => !StageComplete && !string.IsNullOrEmpty(StageTimeRemaining);
		public double StageProgress => TestWell.CurrentStageProgress;
		public string StageStatusText => StageComplete
			? (StageReading is null ? "Stage complete" : $"Stage complete, waiting for reading {StageReading}")
			: (StageReading is null ? StageTimeRemaining : $"Reading {StageReading}: {StageTimeRemaining}");
		public decimal DnaValue { get; set; }
		public string Label => TestWell.Label;
		public string AnimalId => TestWell.AnimalId;
		public TestWell TestWell { get; set; }
		#endregion
	}
}
