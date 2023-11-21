namespace MuchHttp;

public class ConcurrentCounter
{
    private int _value;

    public ConcurrentCounter(int initialValue)
    {
        _value = initialValue;
    }

    public bool TryDecrement()
    {
        return Interlocked.Decrement(ref _value) >= 0;
    }
}