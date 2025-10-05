namespace NxtRemote;

public class Polling<T>(Func<T> pollingFunction, TimeSpan pollingInterval) : IDisposable
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

    public Task WaitForConditionAsync(Func<T, bool> condition)
    {
        TaskCompletionSource taskCompletionSource = new();
        OnDataReceived += DataReceived;
        return taskCompletionSource.Task;

        void DataReceived(object? sender, PollingEventArgs<T> eventArgs)
        {
            if (condition(eventArgs.Data))
            {
                OnDataReceived -= DataReceived;
                taskCompletionSource.TrySetResult();
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

public class PollingEventArgs<T>(T data) : EventArgs
{
    public T Data { get; } = data;
}