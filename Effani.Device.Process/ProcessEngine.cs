using System.Collections.Concurrent;
using static Effani.Device.Control.DeviceControl;

namespace Effani.Device.Process
{
	public class ProcessEngine : IDisposable
	{
		public bool ExecuteProcess()
		{
			if (_processThread is not null)
				return false;

			_processThread = new Thread(ProcessCommands);
			_processThread.Start();
			return true;
		}
		public void TerminateProcess()
		{
			var thread = _processThread;
			_processThread = null;
			thread.Join();
		}

		public bool AddCommand(Command command, bool startProcessing = true)
		{
			CommandQueue.Enqueue(command);
			if (startProcessing)
				return ExecuteProcess();
			return false;
		}
		public string AddCommandAndGetResult(Command command, bool startProcessing = true)
		{
			AddCommand(command);

			for(var x = 0; x < 100; x++)
			{
				if (command.Result is not null)
					return command.Result;
				Thread.Sleep(100);
			}
			return null;
		}
		public void Dispose()
		{
			if(_publishThread is not null)
			{
				_publishThread.Join();
				_publishThread = null;
			}
			if (_processThread is not null)
			{
				_processThread.Join();
				_processThread = null;
			}
		}

		private void ProcessCommands()
		{
			try
			{
				while (CommandQueue.TryDequeue(out var command))
				{
					command.Execute();
					if (!String.IsNullOrEmpty(command.Result))
						EnqueueMessage(command.Result);
				}
				TerminateProcess();
			}
			catch (Exception ex)
			{
				Messages.Enqueue(ex.Message);
			}
		}
		private void PublishMessages()
		{
			while (Messages.TryDequeue(out var message))
			{
				DataReceived?.Invoke(this, new DataArgs<string>(message));
			}
			var thread = _publishThread;
			_publishThread = null;
			thread.Join();
		}
		private void EnqueueMessage(string message)
		{
			Messages.Enqueue(message);
			if (_publishThread is null)
			{
				_publishThread = new Thread(PublishMessages);
				_publishThread.Start();
			}
		}

		public ConcurrentQueue<Command> CommandQueue { get; set; } = new();
		public ConcurrentQueue<string> Messages { get; set; } = new();

		public event EventHandler<DataArgs<string>>? DataReceived;

		Thread _processThread;
		Thread _publishThread;
	}
}
