using System.Diagnostics;
using System.IO.Ports;

namespace NxtRemote;

public class NxtBluetoothCommunication : INxtCommunication
{
    private readonly SerialPort serialPort;

    public NxtBluetoothCommunication(string serialPortName, int timeout = 1000)
    {
        serialPort = new SerialPort(serialPortName)
        {
            ReadTimeout = timeout,
            WriteTimeout = timeout
        };

        serialPort.Open();
    }

    public Task SendWithoutReplyAsync(NxtTelegram telegram)
    {
        Send(telegram, false);
        return Task.CompletedTask;
    }

    public Task<NxtReply> SendWithReplyAsync(NxtTelegram telegram)
    {
        Send(telegram, true);
        return Task.FromResult(ReceiveReply(telegram.Command));
    }

    private void Send(NxtTelegram telegram, bool requestReply)
    {
        if (telegram.Type is not (NxtTelegramType.DirectCommand or NxtTelegramType.SystemCommand))
            throw new ArgumentException("Invalid telegram type for sending.", nameof(telegram));

        var buffer = new byte[telegram.Length + 2];
        buffer[0] = (byte)(telegram.Length & 0xFF);
        buffer[1] = (byte)((telegram.Length & 0xFF00) >> 8);
        telegram.CopyTo(buffer, 2, requestReply);

        if (!IsEstablished)
            throw new NxtCommunicationException("Cannot send telegram because the connection is not established.");

        Write(buffer);
    }

    private NxtReply ReceiveReply(NxtCommand expectedCommand)
    {
        var buffer = Read();

        var reply = new NxtTelegram(buffer);

        if (reply.Type is not NxtTelegramType.Reply)
            throw new NxtCommunicationException($"Invalid telegram type received for reply: {reply.Type}");

        if (reply.Command != expectedCommand)
            throw new NxtCommunicationException($"Reply command does not match sent telegram: {reply.Command}");

        return new NxtReply(reply);
    }

    public void Dispose()
    {
        lock (serialPort)
        {
            serialPort.Close();
            serialPort.Dispose();
        }
    }

    public bool IsEstablished
    {
        get
        {
            lock (serialPort)
            {
                return serialPort is { IsOpen: true, CtsHolding: true };
            }
        }
    }

    private void Write(byte[] buffer)
    {
        lock (serialPort)
            serialPort.Write(buffer, 0, buffer.Length);
    }

    private byte[] Read()
    {
        lock (serialPort)
        {
            var length = serialPort.ReadByte() | serialPort.ReadByte() << 8;
            var buffer = new byte[length];
            serialPort.Read(buffer, 0, length);
            return buffer;
        }
    }
}