using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace MicroserviceB.Api.Telemetry;

public static class DemoTelemetry
{
    public const string Name = "MicroserviceB.Api";
    public static readonly ActivitySource Source = new(Name);           // the stopwatch factory
    private static readonly Meter Meter = new(Name);                    // the tally factory
    public static readonly Counter<long> Failures = Meter.CreateCounter<long>(
        name: "demo.failures",
        unit:"{failure}",
        description: "Simulated failures, tagged by error.type");

}
