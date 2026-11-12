using Drivetrain.Robotics.Control;

namespace Drivetrain.Robotics.Process
{
	/// <summary>
	/// Uploads generated config files to the controller board's system folder, then reboots the board (M999) so it runs the new
	/// config.g. Nothing is rebooted if any upload fails. The outcome (null error = success) is reported to the callback.
	/// </summary>
	public class SendConfigFiles(IEnumerable<string> localPaths, Action<string?> completeCallback, DeviceControl device) : Command(device)
	{
		protected override void Run()
		{
			string? error = null;
			foreach (var path in _localPaths)
			{
				error = Device.SendFile(path, $"0:/sys/{Path.GetFileName(path)}");
				if (error is not null)
				{
					error = $"{Path.GetFileName(path)}: {error}";
					break;
				}
			}
			if (error is null)
				error = Device.Reboot();
			Result = error ?? "Sent";
			try
			{
				_completeCallback?.Invoke(error);
			}
			catch (Exception ex)
			{
				// an exception escaping a command would stop the process engine
				Serilog.Log.Error(ex, "Error reporting config upload result");
			}
		}

		public override string Description => "Send generated config files to the controller board and reboot it.";
		private readonly List<string> _localPaths = localPaths.ToList();
		private readonly Action<string?> _completeCallback = completeCallback;
	}
}
