using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public abstract class ReadSensor(DeviceControl device, Action<ReadSensor> resultCallback) : Command(device)
	{
		protected override void Run()
		{
			Result = device.ReadSensor(SensorDesignator, PauseTime);
			_resultCallback?.Invoke(this);
		}

		public int PauseTime { get; set; }
		public int SensorDesignator { get; set; }

		private Action<ReadSensor> _resultCallback = resultCallback;
	}

}
