namespace NxtRemote;

public interface INxtCommunication : IDisposable
{
    Task SendWithoutReplyAsync(NxtTelegram telegram);
    Task<NxtReply> SendWithReplyAsync(NxtTelegram telegram);
}

public class NxtCommunicationException(string message) : Exception(message);