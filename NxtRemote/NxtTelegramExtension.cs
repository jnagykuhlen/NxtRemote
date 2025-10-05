using System.Runtime.CompilerServices;

namespace NxtRemote;

public static class NxtTelegramExtension
{
    public static NxtTelegram WriteSByte(this NxtTelegram telegram, sbyte value) => telegram.WriteByte((byte)value);
    public static sbyte ReadSByte(this NxtTelegram telegram) => (sbyte)telegram.ReadByte();

    public static NxtTelegram WriteEnum<TEnum>(this NxtTelegram telegram, TEnum value) where TEnum : Enum =>
        telegram.WriteByte(Unsafe.As<TEnum, byte>(ref value));

    public static TEnum ReadEnum<TEnum>(this NxtTelegram telegram) where TEnum : Enum
    {
        var value = telegram.ReadByte();
        return Unsafe.As<byte, TEnum>(ref value);
    }

    public static NxtTelegram WriteInt32(this NxtTelegram telegram, int value) => telegram
        .WriteByte((byte)value)
        .WriteByte((byte)(value >> 8))
        .WriteByte((byte)(value >> 16))
        .WriteByte((byte)(value >> 24));

    public static int ReadInt32(this NxtTelegram telegram) =>
        telegram.ReadByte() |
        (telegram.ReadByte() << 8) |
        (telegram.ReadByte() << 16) |
        (telegram.ReadByte() << 24);

    public static NxtTelegram WriteUInt32(this NxtTelegram telegram, uint value) => telegram.WriteInt32((int)value);
    public static uint ReadUInt32(this NxtTelegram telegram) => (uint)telegram.ReadInt32();
}