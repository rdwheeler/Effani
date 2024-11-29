namespace Effani.Device.Control
{
	public partial class Device
	{
		public class DataArgs<T> : EventArgs
		{
			public DataArgs(T data)
			{
				Data = data;
			}
			public T Data { get; set; }
		}
	}
}
