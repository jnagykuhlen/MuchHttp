using System.Collections.Concurrent;
using System.Diagnostics;

namespace MuchHttp;

public class LoadTest
{
    private const int UpdateProgressIntervalMilliseconds = 200;
    
    private readonly HttpClient _httpClient;
    private readonly string _url;
    private readonly int _concurrentRequests;
    private readonly int _totalRequests;

    public LoadTest(HttpClient httpClient, string url, int concurrentRequests, int totalRequests)
    {
        _httpClient = httpClient;
        _url = url;
        _concurrentRequests = concurrentRequests;
        _totalRequests = totalRequests;
    }

    public async Task<LoadTestResult> Perform(IProgress progress)
    {
        var remainingRequests = _totalRequests;
        var completedRequests = 0;
        var failedRequests = 0;
        var requestTimings = new ConcurrentBag<TimeSpan>();
        
        var updateProgressTask = UpdateProgressAsync();
        var workerTasks = Enumerable.Repeat(ProcessRequestsAsync, _concurrentRequests)
                .Select(taskFactory => taskFactory.Invoke())
                .ToArray();

        await Task.WhenAll(workerTasks);
        await updateProgressTask;

        return new LoadTestResult(completedRequests, failedRequests, requestTimings);
        
        async Task ProcessRequestsAsync()
        {
            var stopwatch = new Stopwatch();
            while (Interlocked.Decrement(ref remainingRequests) + 1 > 0)
            {
                stopwatch.Restart();
                var response = await _httpClient.GetAsync(_url);
                stopwatch.Stop();

                Interlocked.Increment(ref completedRequests);
        
                if (response.IsSuccessStatusCode)
                    requestTimings.Add(stopwatch.Elapsed);
                else
                    Interlocked.Increment(ref failedRequests);
            }
        }

        async Task UpdateProgressAsync()
        {
            await Task.Delay(UpdateProgressIntervalMilliseconds);
            while (completedRequests < _totalRequests)
            {
                progress.Report(completedRequests, _totalRequests);
                await Task.Delay(UpdateProgressIntervalMilliseconds);
            }

            progress.Report(completedRequests, _totalRequests);
            progress.Complete();
        }
    }
}