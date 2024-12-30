using DevExpress.Xpf.Controls;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
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
			var viewModel = DataContext as MainViewModel;
			viewModel.AdvanceWizardPage += ViewModel_AdvanceWizardPage;
			viewModel.PageStartFirstPage();
		}
		private void ViewModel_AdvanceWizardPage(object sender, EventArgs e)
		{
			wizard.GoNext();
		}

		private void Wizard_Next(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var page = wizard.SelectedItem as WizardPage;

			var viewModel = DataContext as MainViewModel;
			switch (page.Name)
			{
				case "GatherParametersPage":
					viewModel.PageStartLoadFluidSystem();
					break;
				case "LoadFluidSystemPage":
					viewModel.PageStartLoadSampleTrays();
					break;
				case "LoadSampleTraysPage":
					viewModel.PageStartLoadingSamples();
					break;
				case "LoadingSamplesPage":
					viewModel.PageStartAssay();
					break;
				case "AssayPage":
					viewModel.PageStartCleanUp();
					break;
				case "CleanUpPage":
					viewModel.PageStartAssayConcluded();
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
	}
}
