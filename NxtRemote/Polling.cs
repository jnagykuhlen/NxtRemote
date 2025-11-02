namespace NxtRemote;

public class Polling<T>(Func<Task<T>> pollAsync, TimeSpan pollingInterval) : IDisposable, IPollable<T>
{
    private event EventHandler<PollingEventArgs<T>>? InternalOnDataReceived;

    private Task? pollingTask;
    private readonly Lock lockObject = new();

    public event EventHandler<PollingEventArgs<T>> OnDataReceived
    {
        add
        {
            lock (lockObject)
            {
                InternalOnDataReceived += value;
                pollingTask ??= CreatePollingTask();
            }
        }
        remove
        {
            lock (lockObject)
            {
                InternalOnDataReceived -= value;
                if (InternalOnDataReceived == null)
                    pollingTask = null;
            }
        }
    }

    private async Task CreatePollingTask()
    {
        while (InternalOnDataReceived != null)
        {
            try
            {
                InternalOnDataReceived?.Invoke(this, new PollingEventArgs<T>(await pollAsync()));
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception while polling data: {exception}");
            }
            
            await Task.Delay(pollingInterval);
        }
    }

    public void Dispose()
    {
        lock (lockObject)
        {
            InternalOnDataReceived = null;
            pollingTask = null;
        }
    }
}