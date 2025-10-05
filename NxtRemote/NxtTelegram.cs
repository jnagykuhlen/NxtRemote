namespace NxtRemote;

public class NxtTelegram
{
    private readonly byte[] buffer;
    private int writePosition;
    private int readPosition;

    public NxtTelegram(byte[] buffer)
    {
        this.buffer = buffer;
        writePosition = buffer.Length;
        readPosition = 2;
    }

    public NxtTelegram(int length, NxtTelegramType type, NxtCommand command)
    {
        if (length < 2)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be at least 2 bytes.");

        buffer = new byte[length];
        readPosition = 2;

        WriteByte((byte)type);
        WriteByte((byte)command);
    }

    public NxtTelegram WriteByte(byte value)
    {
        if (writePosition >= buffer.Length)
            throw new InvalidOperationException("Cannot write beyond telegram buffer.");

        buffer[writePosition++] = value;
        return this;
    }
    
    public byte ReadByte()
    {
        if (readPosition >= writePosition)
            throw new InvalidOperationException("Cannot read beyond telegram buffer.");

        return buffer[readPosition++];
    }

    public void CopyTo(byte[] destination, int startIndex, bool requestReply)
    {
        if (writePosition != buffer.Length)
            throw new InvalidOperationException("Telegram is not fully written.");
        
        if (startIndex < 0 || startIndex + Length > destination.Length)
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Buffer exceeds destination length.");

        Array.Copy(buffer, 0, destination, startIndex, Length);
        if (!requestReply)
            destination[startIndex] |= 0x80;
    }

    public NxtTelegramType Type => (NxtTelegramType)buffer[0];

    public NxtCommand Command => (NxtCommand)buffer[1];

    public int Length => buffer.Length;
}

public enum NxtTelegramType : byte
{
    DirectCommand = 0x00,
    SystemCommand = 0x01,
    Reply = 0x02
}
