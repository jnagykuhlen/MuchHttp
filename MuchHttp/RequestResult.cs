namespace MuchHttp;

public record RequestResult(TimeSpan Timing, string? Error = null)
{
    public bool IsSuccessful => Error is null;
}