using System.IO.Ports;

namespace NxtRemote;

public class NxtBluetoothCommunication : INxtCommunication
{
    private readonly SerialPort serialPort;
    private readonly Lock writeLock = new();
    private readonly Lock readLock = new();
    private Task currentReadTask = Task.CompletedTask;

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
        var buffer = CreateBuffer(telegram, false);
        lock (writeLock)
            WriteToSerialPort(buffer);

        return Task.CompletedTask;
    }

    public Task<NxtReply> SendWithReplyAsync(NxtTelegram telegram)
    {
        var buffer = CreateBuffer(telegram, true);
        lock (writeLock)
        {
            WriteToSerialPort(buffer);

            lock (readLock)
            {
                var readTask = currentReadTask.ContinueWith(_ => ReadReply(telegram.Command));
                currentReadTask = readTask;
                return readTask;
            }
        }
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

    private NxtReply ReadReply(NxtCommand expectedCommand)
    {
        var replyTelegram = new NxtTelegram(ReadFromSerialPort());

        if (replyTelegram.Type is not NxtTelegramType.Reply)
            throw new NxtCommunicationException($"Invalid telegram type received for reply: {replyTelegram.Type}");

        if (replyTelegram.Command != expectedCommand)
            throw new NxtCommunicationException($"Reply command does not match sent telegram: {replyTelegram.Command}");

        return new NxtReply(replyTelegram);
    }

    private byte[] ReadFromSerialPort()
    {
        var length = serialPort.ReadByte() | serialPort.ReadByte() << 8;
        var buffer = new byte[length];
        serialPort.Read(buffer, 0, length);
        return buffer;
    }

    private void WriteToSerialPort(byte[] buffer) => serialPort.Write(buffer, 0, buffer.Length);

    public void Dispose()
    {
        serialPort.Close();
        serialPort.Dispose();
    }
}