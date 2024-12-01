using DevExpress.Xpf.Core;
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
			_viewModel = DataContext as MainViewModel;
		}

		private void Wizard_Cancel(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = (DXMessageBox.Show("Cancel assay and terminate test?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.No);
		}
		private void LoadFluidSystemPage_Loaded(object sender, RoutedEventArgs e)
		{
			_viewModel.LoadFluidSystemPageLoaded();
		}
		private void LoadSampleTraysPage_Loaded(object sender, RoutedEventArgs e)
		{
			_viewModel.LoadSampleTraysPageLoaded();
		}
		private void PreparationPage_Loaded(object sender, RoutedEventArgs e)
		{
			_viewModel.PreparationPageLoaded();
		}
		private void LoadingSamplesPage_Loaded(object sender, RoutedEventArgs e)
		{
			_viewModel.LoadingSamplesPageLoaded();
		}
		private void AssayPage_Loaded(object sender, RoutedEventArgs e)
		{
			_viewModel.AssayPageLoaded();
		}
		private void CleanUpPage_Loaded(object sender, RoutedEventArgs e)
		{
			_viewModel.CleanUpPageLoaded();
		}
		private void AssayConcludedPage_Loaded(object sender, RoutedEventArgs e)
		{
			_viewModel.AssayConcludedPageLoaded();
		}

		MainViewModel _viewModel;
	}
}
