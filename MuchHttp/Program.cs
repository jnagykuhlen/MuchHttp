using System.Globalization;
using MuchHttp;
using MuchHttp.Visualization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

if (args.Length != 3)
{
    Console.WriteLine("Usage: <url> <concurrent requests> <total requests>");
    return;
}

var url = args[0];
var concurrentRequests = int.Parse(args[1]);
var totalRequests = int.Parse(args[2]);

ConsoleBlockWriter consoleWriter = new ConsoleBlockWriter { MaxPropertyWidth = 24 };
consoleWriter.WriteHeading($"Starting load test with {totalRequests} requests ({concurrentRequests} concurrent)");
consoleWriter.WriteSpacing();

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