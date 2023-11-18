using System.CommandLine;
using System.Globalization;
using MuchHttp;
using MuchHttp.Visualization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var urlOption = RequiredOption<Uri>("-u", "--url", "URL", "The URL to direct all HTTP requests to");
var concurrentRequestsOption = RequiredOption<int>("-c", "--concurrent", "concurrent requests", "The maximum number of concurrently sent requests");
var totalRequestsOption = RequiredOption<int>("-n", "--total", "total requests", "The total number of requests to send");

var rootCommand = new RootCommand("Perform HTTP GET requests against a specified URL with a configurable level of concurrency.")
{
    urlOption,
    concurrentRequestsOption,
    totalRequestsOption
};

rootCommand.SetHandler(PerformAsync, urlOption, concurrentRequestsOption, totalRequestsOption);
await rootCommand.InvokeAsync(args);


async Task PerformAsync(Uri url, int concurrentRequests, int totalRequests)
{
    ConsoleBlockWriter consoleWriter = new ConsoleBlockWriter { MaxPropertyWidth = 24 };
    consoleWriter.WriteHeading($"Starting load test with {totalRequests} requests ({concurrentRequests} concurrent)");
    consoleWriter.WriteSpacing();
    
    try
    {
        ConsoleProgressBar progressBar = new ConsoleProgressBar { Width = 64 };

        using var httpClient = new HttpClient();
        var loadTestResult = await new LoadTest(httpClient, url, concurrentRequests, totalRequests).Perform(progressBar);

        consoleWriter.WriteSpacing();
        consoleWriter.WriteHeading("Summary:");
        consoleWriter.WriteProperty("Successful requests", loadTestResult.SuccessfulRequests);
        consoleWriter.WriteProperty("Failed requests", loadTestResult.FailedRequests);
        consoleWriter.WriteProperty("Average", $"{loadTestResult.AverageMilliseconds:N2} ms");
        consoleWriter.WriteProperty("Median", $"{loadTestResult.MedianMilliseconds:N2} ms");
        consoleWriter.WriteProperty("Min", $"{loadTestResult.MinMilliseconds:N2} ms");
        consoleWriter.WriteProperty("Max", $"{loadTestResult.MaxMilliseconds:N2} ms");
        consoleWriter.WriteSpacing();
    }
    catch (Exception exception)
    {
        consoleWriter.WriteException(exception);
        consoleWriter.WriteSpacing();
    }
}

static Option<T> RequiredOption<T>(string alias, string name, string helpName, string description)
{
    return new Option<T>(new [] { alias }, description)
    {
        Name = name,
        ArgumentHelpName = helpName,
        IsRequired = true
    };
}