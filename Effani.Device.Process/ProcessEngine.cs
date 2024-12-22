using Effani.Device.Control;
using Serilog;
using System.Collections.Concurrent;

namespace Effani.Device.Process
{
    public class ProcessEngine : IDisposable
    {
        Thread? _dataPumpThread;

        Thread? _processThread;
        Thread? _publishThread;

        public ProcessEngine()
        {
            _dataPumpThread = new Thread(PublishData);
            _dataPumpThread.Start();
            _processThread = new Thread(ProcessCommands);
            _processThread.Start();
        }

        public event EventHandler<DataArgs<KeyValuePair<string, string>>>? DataReceived;

        public event EventHandler<DataArgs<string>>? MessageReceived;

        private void EnqueueMessage(string message)
        {
            Messages.Enqueue(message);
            if (_publishThread is null)
            {
                _publishThread = new Thread(PublishMessages);
                _publishThread.Start();
            }
        }

        private void ProcessCommands()
        {
            try
            {
                while(true)
                {
                    while (PriorityJobQueue.TryDequeue(out var job))
                    {
                        foreach (var command in job.Commands)
                        {
                            command.Execute();
                            if (!String.IsNullOrEmpty(command.Result))
                                EnqueueMessage(command.Result);
                        }
                    }
                    while (!HoldQueueForHighPriorityJob && JobQueue.TryDequeue(out var job))
                    {
                        foreach (var command in job.Commands)
                        {
                            command.Execute();
                            if (!String.IsNullOrEmpty(command.Result))
                                EnqueueMessage(command.Result);
                            Log.Information($"{command.GetType().Name}:{command.Result}");
                        }
                        if (PriorityJobQueue.Any())
                            break;
                    }
                    Thread.Sleep(500);
                    //TerminateProcess();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception executing command");
                Messages.Enqueue(ex.Message);
                TerminateProcess();
            }
        }
        private void PublishData()
        {
            while (Data.TryDequeue(out var message))
            {
                foreach (var token in ReadTokens.ToList())
                {
                    if (message.Contains(token.Value))
                    {
                        DataReceived?.Invoke(this, new DataArgs<KeyValuePair<string, string>>(new KeyValuePair<string, string>(token.Key, message)));
                        ReadTokens.TryRemove(token);
                    }
                }
            }
            Task.Delay(1000).Wait();
        }
        private void PublishMessages()
        {
            while (Messages.TryDequeue(out var message))
            {
                MessageReceived?.Invoke(this, new DataArgs<string>(message));
            }
            //var thread = _publishThread;
            //_publishThread = null;
            //thread.Join();
        }

        public void Dispose()
        {
            if (_publishThread is not null)
            {
                _publishThread.Join();
                _publishThread = null;
            }
            if (_processThread is not null)
            {
                _processThread.Join();
                _processThread = null;
            }
            if (_dataPumpThread is not null)
            {
                _dataPumpThread.Join();
                _dataPumpThread = null;
            }
        }

        public bool ExecuteProcess()
        {
            //if (_processThread is not null)
            //    return false;

            //_processThread = new Thread(ProcessCommands);
            //_processThread.Start();

            return true;
        }

        public bool QueueCommand(Command command)
        {
            return QueueJob(new Job(command.Description, [command]));
        }

        public bool QueueJob(Job job)
        {
                JobQueue.Enqueue(job);
                return false;
        }
        public bool QueuePriorityJob(Job job)
        {
            PriorityJobQueue.Enqueue(job);
            Task.Delay(1000).Wait();
            return false;
        }

        public string QueueJobForResult(Job job)
        {
            QueueJob(job);
            return null;

            //ProcessingLock.EnterWriteLock();
            //try
            //{
            //    for (int i = 0; i < job.Commands.Count() - 1; i++) // presume last command is the one we want results from
            //    {
            //        job.Commands[i].Execute();
            //    }
            //    var readCommand = job.Commands.Last();
            //    readCommand.Execute();
            //    return readCommand.Result;
            //}
            //finally
            //{
            //    ProcessingLock.ExitWriteLock();
            //}
        }
        public void TerminateProcess()
        {
            var thread = _processThread;
            _processThread = null;
            thread.Join();
        }

        public ConcurrentQueue<Job> JobQueue { get; } = new();
        public ConcurrentQueue<Job> PriorityJobQueue { get; } = new();
        public ConcurrentQueue<string> Data { get; } = new();
        public ConcurrentQueue<string> Messages { get; } = new();
        public bool HoldQueueForHighPriorityJob { get; set; }
        public ConcurrentDictionary<string, string> ReadTokens { get; } = new();
    }
}
