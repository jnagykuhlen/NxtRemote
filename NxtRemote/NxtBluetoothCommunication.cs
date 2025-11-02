using System.IO.Ports;

namespace NxtRemote;

public class NxtBluetoothCommunication : INxtCommunication
{
    private readonly SerialPort serialPort;
    private readonly Lock writeLock = new();
    private readonly Lock readLock = new();
    private readonly Queue<PendingReply> pendingReplies = new();
    private Task? readTask;

    public NxtBluetoothCommunication(string serialPortName, int timeout = 1000)
    {
        serialPort = new SerialPort(serialPortName)
        {
            ReadTimeout = timeout,
            WriteTimeout = timeout
        };

        serialPort.Open();
        
        if (serialPort is not { IsOpen: true, CtsHolding: true })
            throw new NxtCommunicationException("Failed to establish serial port.");
    }

    public Task SendWithoutReplyAsync(NxtTelegram telegram)
    {
        WriteWithoutReply(CreateBuffer(telegram, false));
        return Task.CompletedTask;
    }

    public Task<NxtReply> SendWithReplyAsync(NxtTelegram telegram)
    {
        var taskCompletionSource = new TaskCompletionSource<NxtReply>();

        WriteWithReply(
            CreateBuffer(telegram, true),
            new PendingReply(telegram.Command, taskCompletionSource)
        );

        return taskCompletionSource.Task;
    }

    private static byte[] CreateBuffer(NxtTelegram telegram, bool requestReply)
    {
        if (telegram.Type is not (NxtTelegramType.DirectCommand or NxtTelegramType.SystemCommand))
            throw new ArgumentException("Invalid telegram type for sending.", nameof(telegram));

        var buffer = new byte[telegram.Length + 2];
        buffer[0] = (byte)(telegram.Length & 0xFF);
        buffer[1] = (byte)((telegram.Length & 0xFF00) >> 8);
        telegram.CopyTo(buffer, 2, requestReply);
        return buffer;
    }

    public void Dispose()
    {
        serialPort.Close();
        serialPort.Dispose();
    }

    private void WriteWithoutReply(byte[] buffer)
    {
        lock (writeLock)
            serialPort.Write(buffer, 0, buffer.Length);
    }

    private void WriteWithReply(byte[] buffer, PendingReply pendingReply)
    {
        lock (writeLock)
        {
            serialPort.Write(buffer, 0, buffer.Length);
            
            lock (readLock)
            {
                pendingReplies.Enqueue(pendingReply);
                readTask ??= Task.Factory.StartNew(ReadPendingReplies, TaskCreationOptions.LongRunning);
            }
        }
    }

    private void ReadPendingReplies()
    {
        while (GetNextPendingReply() is { } nextPendingReply)
        {
            try
            {
                var reply = ReadReply(nextPendingReply.ExpectedCommand);
                nextPendingReply.CompletionSource.SetResult(reply);
            }
            catch (Exception exception)
            {
                nextPendingReply.CompletionSource.SetException(exception);
            }
        }

        lock (readLock)
            readTask = null;
    }

    private PendingReply? GetNextPendingReply()
    {
        lock (readLock)
        {
            Console.WriteLine($"Currently {pendingReplies.Count} pending replies.");
            pendingReplies.TryDequeue(out var pendingReply);
            return pendingReply;
        }
    }

    private NxtReply ReadReply(NxtCommand expectedCommand)
    {
        var replyTelegram = new NxtTelegram(Read());

        if (replyTelegram.Type is not NxtTelegramType.Reply)
            throw new NxtCommunicationException($"Invalid telegram type received for reply: {replyTelegram.Type}");

        if (replyTelegram.Command != expectedCommand)
            throw new NxtCommunicationException($"Reply command does not match sent telegram: {replyTelegram.Command}");

        return new NxtReply(replyTelegram);
    }

    private byte[] Read()
    {
            var length = serialPort.ReadByte() | serialPort.ReadByte() << 8;
            var buffer = new byte[length];
            serialPort.Read(buffer, 0, length);
            return buffer;
    }

    private record PendingReply(NxtCommand ExpectedCommand, TaskCompletionSource<NxtReply> CompletionSource);
}