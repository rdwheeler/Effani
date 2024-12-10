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

		public bool QueueCommands(CommandPacket commandPacket, bool startProcessing = true)
		{
			ProcessingLock.EnterWriteLock();
			try
			{
				CommandPacketQueue.Enqueue(commandPacket);
				if (startProcessing)
					return ExecuteProcess();
				return false;
			}
			finally
			{
				ProcessingLock.ExitWriteLock();
			}
		}

		public bool QueueCommand(Command command, bool startProcessing = true)
		{
			return QueueCommands(new CommandPacket([command]), startProcessing);
		}

		public string ExecuteCommandsAndGetResult(CommandPacket commandPacket, bool startProcessing = true)
		{
			ProcessingLock.EnterWriteLock();
			try
			{
				for (int i = 0; i < commandPacket.Commands.Count() - 1; i++) // presume last command is the one we want results from
				{
					commandPacket.Commands[i].Execute();
				}
				var readCommand = commandPacket.Commands.Last();
				readCommand.Execute();
				return readCommand.Result;
			}
			finally
			{
				ProcessingLock.ExitWriteLock();
			}
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
				while (CommandPacketQueue.TryDequeue(out var commandPacket))
				{
					foreach(var command in commandPacket.Commands)
					{
						command.Execute();
						if (!String.IsNullOrEmpty(command.Result))
							EnqueueMessage(command.Result);
					}
				}
				TerminateProcess();
			}
			catch (Exception ex)
			{
				Messages.Enqueue(ex.Message);
                TerminateProcess();
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

		public ConcurrentQueue<CommandPacket> CommandPacketQueue { get; } = new();
		public ConcurrentQueue<string> Messages { get; } = new();
		public ReaderWriterLockSlim ProcessingLock { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

		public event EventHandler<DataArgs<string>>? DataReceived;

		Thread? _processThread;
		Thread? _publishThread;
	}
}
