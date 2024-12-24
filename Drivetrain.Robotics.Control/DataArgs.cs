namespace Drivetrain.Robotics.Control
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
