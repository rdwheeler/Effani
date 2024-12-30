using Drivetrain.Robotics.Control;
using Serilog;
using System.Collections.Concurrent;

namespace Drivetrain.Robotics.Process
{
	public class ProcessEngine : IDisposable
	{
		#region Constructors
		public ProcessEngine()
		{
			_processThread = new Thread(ProcessCommands);
			_processThread.Start();
			_publishThread = new Thread(PublishMessages);
			_publishThread.Start();
		}
		#endregion

		#region Public methods
		public void Dispose()
		{
			TerminateProcess();
			_publishThread = null;
			_processThread = null;
		}
		public void QueueCommand(Command command)
		{
			QueueJob(new Job(null, [command]));
		}
		public void QueuePriorityCommand(Command command)
		{
			QueuePriorityJob(new Job(null, [command]));
		}
		public void QueueJob(Job job)
		{
			JobQueue.Enqueue(job);
		}
		public void QueuePriorityJob(Job job)
		{
			PriorityJobQueue.Enqueue(job);
		}
		public void TerminateProcess()
		{
			_stopEngine = true;
		}
		#endregion

		#region Private methods
		private void DoJob(Job job)
		{
			if (job.Description is not null)
				EnqueueMessage($"Begin {job.Description}");

			foreach (var command in job.Commands)
			{
				var descriptionPrefix = job.Description is null ? String.Empty : "     ";
				EnqueueMessage($"{descriptionPrefix}{command.Description}");
				BeginExecutingCommand?.Invoke(this, new DataArgs<Command>(command));
				command.Execute();
				EndExecutingCommand?.Invoke(this, new DataArgs<Command>(command));
				if (command is ReadSensor)
					EnqueueMessage($"{descriptionPrefix}Result of {command.Description}: {command.Result}");
			}

			if (job.Description is not null)
				EnqueueMessage($"End {job.Description}");
		}
		private void EnqueueMessage(string message)
		{
			Messages.Enqueue(message);
		}
		private void ProcessCommands()
		{
			try
			{
				while (!_stopEngine)
				{
					while (PriorityJobQueue.TryDequeue(out var job))
					{
						DoJob(job);
					}
					while (!HoldQueueForHighPriorityJob && JobQueue.TryDequeue(out var job))
					{
						DoJob(job);
						if (PriorityJobQueue.Any())
							break;
					}
					Thread.Sleep(100);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Exception executing command");
				Messages.Enqueue(ex.Message);
			}
		}
		private void PublishMessages()
		{
			while (!_stopEngine)
			{
				while (Messages.TryDequeue(out var message))
				{
					MessageReceived?.Invoke(this, new DataArgs<string>(message));
				}
				Thread.Sleep(500);
			}
		}
		#endregion

		#region Properties
		public ConcurrentQueue<string> Data { get; } = new();
		public bool HoldQueueForHighPriorityJob { get; set; }
		public ConcurrentQueue<Job> JobQueue { get; } = new();
		public ConcurrentQueue<string> Messages { get; } = new();
		public ConcurrentQueue<Job> PriorityJobQueue { get; } = new();
		#endregion

		#region Events
		public event EventHandler<DataArgs<KeyValuePair<string, string>>>? DataReceived;
		public event EventHandler<DataArgs<string>>? MessageReceived;
		public event EventHandler<DataArgs<Command>>? BeginExecutingCommand;
		public event EventHandler<DataArgs<Command>>? EndExecutingCommand;
		#endregion

		#region Fields
		Thread? _processThread;
		Thread? _publishThread;
		private bool _stopEngine;
		#endregion
	}
}
