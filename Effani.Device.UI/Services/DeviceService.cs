using Effani.Device.Control;
using Effani.Device.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Effani.Device.UI
{
	public class DeviceService : IDeviceService
	{
		public DeviceService()
		{
			_device = new DeviceControl();
			_processEngine = new ProcessEngine();
		}

		public DeviceControl GetDevice()
		{
			return _device;
		}
		public ProcessEngine GetProcessEngine()
		{
			return _processEngine;
		}

		private DeviceControl _device;
		private ProcessEngine _processEngine;
	}

	public interface IDeviceService
	{
		ProcessEngine GetProcessEngine();
		DeviceControl GetDevice();
	}
}
