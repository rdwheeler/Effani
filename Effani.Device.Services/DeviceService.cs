using Drivetrain.Robotics.Control;
using Drivetrain.Robotics.Process;
using Effani.Device.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.Services
{
	public class DeviceService : IDeviceService
	{
		public DeviceService(ISettingsService settingsService)
		{
			_device = new AbalDeviceControl(settingsService.Settings);
			_processEngine = new ProcessEngine();
		}

		public AbalDeviceControl GetDevice()
		{
			return _device;
		}
		public ProcessEngine GetProcessEngine()
		{
			return _processEngine;
		}

		private AbalDeviceControl _device;
		private ProcessEngine _processEngine;
	}

	public interface IDeviceService
	{
		ProcessEngine GetProcessEngine();
		AbalDeviceControl GetDevice();
	}
}
