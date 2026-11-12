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
			_device = new EffaniDeviceControl(() => settingsService.Settings);
			_device.Initialize();
			_processEngine = new ProcessEngine();
		}

		public EffaniDeviceControl GetDevice()
		{
			return _device;
		}
		public ProcessEngine GetProcessEngine()
		{
			return _processEngine;
		}

		private EffaniDeviceControl _device;
		private ProcessEngine _processEngine;
	}

	public interface IDeviceService
	{
		ProcessEngine GetProcessEngine();
		EffaniDeviceControl GetDevice();
	}
}
