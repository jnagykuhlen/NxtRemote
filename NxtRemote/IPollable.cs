namespace NxtRemote;

public interface IPollable<T>
{
    T PollOnce();
    event EventHandler<PollingEventArgs<T>> OnDataReceived;

    public Task WhenAsync(Func<T, bool> predicate)
    {
        TaskCompletionSource taskCompletionSource = new();
        OnDataReceived += DataReceived;
        return taskCompletionSource.Task;

        void DataReceived(object? sender, PollingEventArgs<T> eventArgs)
        {
            if (predicate(eventArgs.Data))
            {
                OnDataReceived -= DataReceived;
                taskCompletionSource.TrySetResult();
            }
        }
    }
}

public class PollingEventArgs<T>(T data) : EventArgs
{
    public T Data { get; } = data;
}
