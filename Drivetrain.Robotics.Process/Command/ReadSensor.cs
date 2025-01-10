using Drivetrain.Robotics.Control;
using System;
using System.Linq;

namespace Drivetrain.Robotics.Process
{
	public class ReadSensor(DeviceControl device, Action<ReadSensor> resultCallback) : Command(device)
	{
		protected override void Run()
		{
			Result = device.ReadSensor(SensorDesignator, PauseTime);
			_resultCallback?.Invoke(this);
		}

		public int PauseTime { get; set; }
		public int SensorDesignator { get; set; }
		public override string Description => $"Read sensor {SensorDesignator}";

		private Action<ReadSensor> _resultCallback = resultCallback;
	}

}
