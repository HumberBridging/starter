namespace MicroserviceA.Api.Clients;

/// <summary>
/// Typed HttpClient for MicroserviceB. The base address (and, later, resilience) is configured in Program.cs.
/// </summary>
public class MicroserviceBClient
{
    private readonly HttpClient _http;

    public MicroserviceBClient(HttpClient http)
    {
        _http = http;
    }

    public Task<HttpResponseMessage> GetUnstableAsync(string? mode, CancellationToken cancellationToken) =>
        _http.GetAsync(WithMode("api/demo/unstable", mode), cancellationToken);

    public Task<HttpResponseMessage> GetSlowAsync(int ms, CancellationToken cancellationToken) =>
        _http.GetAsync($"api/demo/slow?ms={ms}", cancellationToken);

    public Task<HttpResponseMessage> CreateOrderAsync(string? mode, CancellationToken cancellationToken) =>
        _http.PostAsync(WithMode("api/demo/orders", mode), content: null, cancellationToken);

    private static string WithMode(string path, string? mode) =>
        string.IsNullOrWhiteSpace(mode) ? path : $"{path}?mode={Uri.EscapeDataString(mode)}";
}
