using DevExpress.Xpf.Core;
using Effani.Device.Assay;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Effani.Device.UI
{
	public partial class UploadView : ThemedWindow
	{
		public UploadView(IAssayManagerService assayManagerService)
		{
			InitializeComponent();
			DataContext = new UploadViewModel(assayManagerService);
			this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.90);
			this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.90);
		}
	}
}
