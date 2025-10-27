namespace NxtRemote;

public interface IPollable<T>
{
    event EventHandler<PollingEventArgs<T>> OnDataReceived;

    public Task<T> WhenAsync(Func<T, bool> predicate)
    {
        TaskCompletionSource<T> taskCompletionSource = new();
        OnDataReceived += DataReceived;
        return taskCompletionSource.Task;

        void DataReceived(object? sender, PollingEventArgs<T> eventArgs)
        {
            if (predicate(eventArgs.Data))
            {
                OnDataReceived -= DataReceived;
                taskCompletionSource.TrySetResult(eventArgs.Data);
            }
        }
    }
    
    public Task<TSelected> WhenAsync<TSelected>(Func<T, TSelected> selector, Func<TSelected, bool> predicate) =>
        WhenAsync(data => predicate(selector(data))).ContinueWith(task => selector(task.Result));
    
    public Task<T> NextAsync() => WhenAsync(_ => true);
    
    public Task<TSelected> NextAsync<TSelected>(Func<T, TSelected> selector) =>
        NextAsync().ContinueWith(task => selector(task.Result));
}

public class PollingEventArgs<T>(T data) : EventArgs
{
    public T Data { get; } = data;
}
