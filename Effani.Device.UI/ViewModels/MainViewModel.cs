using DevExpress.Mvvm.CodeGenerators;
using System.Collections.ObjectModel;

namespace Effani.Device.UI
{
	[GenerateViewModel]
	public partial class MainViewModel
	{
		public ObservableCollection<PageViewModel> Pages { get; set; }
		public MainViewModel()
		{
			Pages = new ObservableCollection<PageViewModel>() {
				new PageViewModel() { Text = "Welcome Page", Type = PageType.Welcome },
				new PageViewModel() { Text = "Common Page", Type = PageType.Common },
				new PageViewModel() { Text = "Completion Page", Type = PageType.Completion },
			};
		}
	}

	public enum PageType { Welcome, Common, Completion }
	public class PageViewModel
	{
		public string Text { get; set; }
		public PageType Type { get; set; }
	}
}
