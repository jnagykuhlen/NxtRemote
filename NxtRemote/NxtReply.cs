namespace NxtRemote;

public class NxtReply(NxtTelegram telegram)
{
    public NxtCommandStatus Status { get; } = (NxtCommandStatus)telegram.ReadByte();

    public NxtTelegram Success()
    {
        if (Status != NxtCommandStatus.Success)
            throw new NxtCommunicationException($"Command failed with status \"{Status}\".");

        return telegram;
    }
}