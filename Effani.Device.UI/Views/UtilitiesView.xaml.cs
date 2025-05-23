using DevExpress.Xpf.Core;
using System;
using System.Linq;
using System.Windows.Controls;

namespace Effani.Device.UI
{
	public partial class UtilitiesView : ThemedWindow
	{
		public UtilitiesView()
		{
			InitializeComponent(); 
			this.Height = (System.Windows.SystemParameters.PrimaryScreenHeight * 0.90);
			this.Width = (System.Windows.SystemParameters.PrimaryScreenWidth * 0.90);
		}
	}
}
