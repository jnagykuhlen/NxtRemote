namespace NxtRemote;

public interface INxtCommunication : IDisposable
{
    void SendWithoutReply(NxtTelegram telegram);
    NxtReply SendWithReply(NxtTelegram telegram);
}

public class NxtCommunicationException(string message) : Exception(message);