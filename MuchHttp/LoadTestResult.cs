using System.Collections.Concurrent;

namespace MuchHttp;

public class LoadTestResult
{
    public int CompletedRequests { get; }
    public int FailedRequests { get; }

    private readonly List<double> _sortedRequestMilliseconds;

    public LoadTestResult(int completedRequests, int failedRequests, ConcurrentBag<TimeSpan> requestTimings)
    {
        CompletedRequests = completedRequests;
        FailedRequests = failedRequests;
        
        _sortedRequestMilliseconds = requestTimings
            .Select(requestTiming => requestTiming.TotalMilliseconds)
            .ToList();
        _sortedRequestMilliseconds.Sort();
        
        if (_sortedRequestMilliseconds.Count == 0)
            throw new ArgumentException("Must provide at least one timing value.", nameof(requestTimings));
    }

    public int SuccessfulRequests => CompletedRequests - FailedRequests;

    public double AverageMilliseconds => _sortedRequestMilliseconds.Average();
    public double MedianMilliseconds => _sortedRequestMilliseconds[_sortedRequestMilliseconds.Count / 2];
    public double MinMilliseconds => _sortedRequestMilliseconds[0];
    public double MaxMilliseconds => _sortedRequestMilliseconds[^1];
}