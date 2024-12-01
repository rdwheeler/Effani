namespace Effani.Device.Control
{
	public partial class DeviceControl
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
