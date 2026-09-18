using System.ComponentModel.DataAnnotations;

namespace MicroserviceB.Api.Chaos;

/// <summary>
/// Controls how badly MicroserviceB behaves. Bound from the "Chaos" configuration section.
/// </summary>
public sealed class ChaosOptions
{
    public const string SectionName = "Chaos";

    /// <summary>Chance (0.0 – 1.0) that a "random" request succeeds. Default 0.3 = 70% failures.</summary>
    [Range(0.0, 1.0)]
    public double SuccessRate { get; set; } = 0.3;
}
