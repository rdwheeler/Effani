using System.Collections.Concurrent;
using static Effani.Device.Control.DeviceControl;

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
                while (CommandPacketQueue.TryDequeue(out var commandPacket))
                {
                    foreach (var command in commandPacket.Commands)
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
            Task.Delay(500).Wait();
        }
        private void PublishMessages()
        {
            while (Messages.TryDequeue(out var message))
            {
                MessageReceived?.Invoke(this, new DataArgs<string>(message));
            }
            var thread = _publishThread;
            _publishThread = null;
            thread.Join();
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
            if (_processThread is not null)
                return false;

            _processThread = new Thread(ProcessCommands);
            _processThread.Start();
            return true;
        }

        public bool QueueCommand(Command command, bool startProcessing = true)
        {
            return QueueCommands(new CommandPacket([command]), startProcessing);
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

        public bool QueueCommandsForResult(CommandPacket commandPacket, bool startProcessing = true)
        {
            var lastCommand = commandPacket.Commands.Last();
            ReadTokens.TryAdd(lastCommand.ResultDataToken.Key, lastCommand.ResultDataToken.Value);
            return QueueCommands(commandPacket, startProcessing);
        }
        public void TerminateProcess()
        {
            var thread = _processThread;
            _processThread = null;
            thread.Join();
        }

        public ConcurrentQueue<CommandPacket> CommandPacketQueue { get; } = new();
        public ConcurrentQueue<string> Data { get; } = new();
        public ConcurrentQueue<string> Messages { get; } = new();
        public ReaderWriterLockSlim ProcessingLock { get; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public ConcurrentDictionary<string, string> ReadTokens { get; } = new();
    }
}
