namespace Main.Core;

public sealed class Signal<T>
{
    private readonly List<Action<T>> _handlers = new();

    public IDisposable Subscribe(Action<T> handler)
    {
        _handlers.Add(handler);
        return new Unsubscriber(() => _handlers.Remove(handler));
    }

    public void Publish(T value)
    {
        foreach (var handler in _handlers.ToArray())
            handler(value);
    }

    private sealed class Unsubscriber : IDisposable
    {
        private readonly Action _unsubscribe;
        public Unsubscriber(Action unsubscribe) => _unsubscribe = unsubscribe;
        public void Dispose() => _unsubscribe?.Invoke();
    }
}