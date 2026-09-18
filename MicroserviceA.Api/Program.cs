using MicroserviceA.Api.Clients;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Resources;

namespace MicroserviceA.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //telemetry
            var telemetryNamespace = builder.Configuration["Telemetry:Namespace"];

            var otel = builder.Services.AddOpenTelemetry()
                                .ConfigureResource(resource => resource.AddService(
                                 serviceName: "MicroserviceA.Api",
                                 serviceNamespace: telemetryNamespace)
                                );

            if (!string.IsNullOrEmpty(builder.Configuration["AzureMonitor:ConnectionString"]))
            {
                otel.UseAzureMonitor();
            }

            // Typed HttpClient for MicroserviceB: the address comes from configuration, not code
            var microserviceBAddress = builder.Configuration["Services:MicroserviceB"]
                ?? throw new InvalidOperationException("Configuration value 'Services:MicroserviceB' is missing.");

            builder.Services.AddHttpClient<MicroserviceBClient>(client =>
                    client.BaseAddress = new Uri(microserviceBAddress));

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();   // GET /openapi/v1.json
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
