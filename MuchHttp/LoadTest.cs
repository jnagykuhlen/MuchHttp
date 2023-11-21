using System.Collections.Concurrent;
using System.Diagnostics;

namespace MuchHttp;

public class LoadTest
{
    private const int UpdateProgressIntervalMilliseconds = 100;
    
    private readonly HttpClient _httpClient;
    private readonly Uri _url;
    private readonly int _concurrentRequests;
    private readonly int _totalRequests;

    public LoadTest(HttpClient httpClient, Uri url, int concurrentRequests, int totalRequests)
    {
        _httpClient = httpClient;
        _url = url;
        _concurrentRequests = Math.Min(concurrentRequests, totalRequests);
        _totalRequests = totalRequests;
    }

    public async Task<LoadTestResult> PerformAsync(IProgress progress)
    {
        var remainingRequests = new ConcurrentCounter(_totalRequests);
        var requestResults = new ConcurrentBag<RequestResult>();
        
        var updateProgressTask = UpdateProgressAsync();
        var workerTasks = Enumerable.Repeat(ProcessRequestsAsync, _concurrentRequests)
                .Select(taskFactory => taskFactory.Invoke())
                .ToArray();

        await Task.WhenAll(workerTasks);
        await updateProgressTask;

        return new LoadTestResult(requestResults);
        
        async Task ProcessRequestsAsync()
        {
            while (remainingRequests.TryDecrement())
            {
                var requestResult = await ProcessRequestAsync();
                requestResults.Add(requestResult);
            }
        }

        async Task UpdateProgressAsync()
        {
            await Task.Delay(UpdateProgressIntervalMilliseconds);
            while (requestResults.Count < _totalRequests)
            {
                progress.Report(requestResults.Count, _totalRequests);
                await Task.Delay(UpdateProgressIntervalMilliseconds);
            }

            progress.Report(_totalRequests, _totalRequests);
            progress.Complete();
        }
    }

    private async Task<RequestResult> ProcessRequestAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _httpClient.GetAsync(_url);
            stopwatch.Stop();

            if (response.IsSuccessStatusCode)
                return new RequestResult(stopwatch.Elapsed);

            return new RequestResult(stopwatch.Elapsed, $"HTTP status {response.StatusCode}");
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return new RequestResult(stopwatch.Elapsed, $"{exception.GetType().Name}: {exception.Message}");
        }
    }
}