namespace Effani.Device.UI
{
	public class PageViewModel
	{
		public string Text { get; set; }

		public PageType Type { get; set; }
	}
	public enum PageType
	{
		Welcome,
		Common,
		Completion
	}
}
