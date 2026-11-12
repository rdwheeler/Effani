using DevExpress.Xpf.Controls;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using DevExpress.XtraScheduler.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Effani.Device.UI
{
    /// <summary>
    /// Interaction logic for Wizard.xaml
    /// </summary>
    public partial class Wizard : UserControl
    {
        public Wizard()
        {
            InitializeComponent();
			//camera.DeviceInfo = camera.GetAvailableDevices().FirstOrDefault(c => c.Name == "Integrated Webcam");
			Loaded += Wizard_Loaded;
		}

		private void Wizard_Loaded(object sender, RoutedEventArgs e)
		{
			ViewModel.AdvanceWizardPage += ViewModel_AdvanceWizardPage;
			ViewModel.ResetWizardPage -= ViewModel_ResetWizardPage;
			ViewModel.ResetWizardPage += ViewModel_ResetWizardPage;
			ViewModel.GoToCleanUpPage -= ViewModel_GoToCleanUpPage;
			ViewModel.GoToCleanUpPage += ViewModel_GoToCleanUpPage;
		}
		private void ViewModel_GoToCleanUpPage(object sender, EventArgs e)
		{
			ViewModel.PageStartCleanUp();
			wizard.SelectedItem = CleanUpPage;
		}
		private void ViewModel_ResetWizardPage(object sender, EventArgs e)
		{
			// the first page's editors only push their values to the view model, so clear them here
			HerdIdTextEdit.EditValue = null;
			NumberOfSamplesEdit.EditValue = null;
			wizard.SelectedIndex = 0;
		}
		private void ViewModel_AdvanceWizardPage(object sender, EventArgs e)
		{
			wizard.GoNext();
		}

		private void Wizard_Next(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var page = wizard.SelectedItem as WizardPage;
			switch (page.Name)
			{
				case "GatherParametersPage":
					ViewModel.PageStartLoadFluidSystem();
					break;
				case "LoadFluidSystemPage":
					ViewModel.PageStartLoadingSamples();
					break;
				case "AssayPage":
					ViewModel.PageStartCleanUp();
					break;
				case "CleanUpPage":
					ViewModel.PageStartAssayConcluded();
					break;
			}
		}
		private void Wizard_Cancel(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = (DXMessageBox.Show("Are you sure you want to cancel this assay?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No);
			if(e.Cancel)
			{
				(DataContext as MainViewModel).CancelAssay();
			}
		}

        private void AnimalIdTextEdit_Validate(object sender, ValidationEventArgs e)
        {
			if (ViewModel != null && !String.IsNullOrWhiteSpace((string)e.Value))
				ViewModel.InjectSample();
        }
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            _backgroundPlayer.Position = TimeSpan.Zero;
            _backgroundPlayer.Play();
        }
        private void GetFocus(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement element && element.Visibility == Visibility.Visible)
            {
                element.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Input,
                    new Action(() => element.Focus()));
            }
        }

        public MainViewModel ViewModel => DataContext as MainViewModel;
    }
}
