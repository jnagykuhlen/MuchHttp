﻿using System.CommandLine;
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
return await rootCommand.InvokeAsync(args);


async Task PerformAsync(Uri url, int concurrentRequests, int totalRequests)
{
    ConsoleBlock.Colored(ConsoleColor.Cyan, () =>
        ConsoleBlock.FromHeading($"Starting load test with {totalRequests} requests ({concurrentRequests} concurrent)")
    );

    try
    {
        var progressBar = new ConsoleProgressBar { Width = 64 };

        using var httpClient = new HttpClient();
        var loadTestResult = await new LoadTest(httpClient, url, concurrentRequests, totalRequests).PerformAsync(progressBar);

        ConsoleBlock.Create(24, "Summary:", block =>
        {
            block.WriteProperty("Successful requests", loadTestResult.SuccessfulRequests);
            block.WriteProperty("Failed requests", loadTestResult.FailedRequests);
            block.WriteProperty("Average", $"{loadTestResult.AverageMilliseconds:N2} ms");
            block.WriteProperty("Median", $"{loadTestResult.MedianMilliseconds:N2} ms");
            block.WriteProperty("Min", $"{loadTestResult.MinMilliseconds:N2} ms"); 
            block.WriteProperty("Max", $"{loadTestResult.MaxMilliseconds:N2} ms");
        });

        if (loadTestResult.FailedRequests > 0)
        {
            ConsoleBlock.Colored(ConsoleColor.Red, () =>
            {
                ConsoleBlock.Create(64, "Errors:", block =>
                {
                    foreach (var error in loadTestResult.AggregatedErrors)
                        block.WriteProperty(error.Message, error.Count);
                });
            });
        }
    }
    catch (Exception exception)
    {
        ConsoleBlock.Colored(ConsoleColor.Red, () =>
            ConsoleBlock.FromException(exception)
        );
    }
}

static Option<T> RequiredOption<T>(string alias, string name, string helpName, string description)
{
    return new Option<T>(new[] { alias }, description)
    {
        Name = name,
        ArgumentHelpName = helpName,
        IsRequired = true
    };
}