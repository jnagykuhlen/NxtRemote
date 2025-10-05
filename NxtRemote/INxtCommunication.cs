namespace NxtRemote;

public interface INxtCommunication : IDisposable
{
    void SendWithoutReply(NxtTelegram telegram);
    NxtTelegram SendWithReply(NxtTelegram telegram);
}

public class NxtCommunicationException(string message) : Exception(message);
