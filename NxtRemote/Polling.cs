namespace NxtRemote;

public class Polling<T>(Func<T> pollingFunction, TimeSpan pollingInterval) : IDisposable, IPollable<T>
{
    private event EventHandler<PollingEventArgs<T>>? InternalOnDataReceived;

    private Timer? timer;
    private readonly Lock lockObject = new();

    public event EventHandler<PollingEventArgs<T>> OnDataReceived
    {
        add
        {
            lock (lockObject)
            {
                InternalOnDataReceived += value;
                timer ??= new Timer(
                    OnTimerCallback,
                    null,
                    TimeSpan.Zero,
                    pollingInterval
                );
            }
        }
        remove
        {
            lock (lockObject)
            {
                InternalOnDataReceived -= value;
                if (InternalOnDataReceived == null)
                {
                    timer?.Dispose();
                    timer = null;
                }
            }
        }
    }

    private void OnTimerCallback(object? state)
    {
        if (lockObject.TryEnter())
        {
            try
            {
                InternalOnDataReceived?.Invoke(this, new PollingEventArgs<T>(pollingFunction()));
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Exception while polling data: {exception}");
            }
            finally
            {
                lockObject.Exit();
            }
        }
    }

    public void Dispose()
    {
        lock (lockObject)
        {
            InternalOnDataReceived = null;
            timer?.Dispose();
            timer = null;
        }
    }
}