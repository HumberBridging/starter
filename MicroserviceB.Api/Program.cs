using Azure.Monitor.OpenTelemetry.AspNetCore;
using MicroserviceB.Api.Chaos;
using MicroserviceB.Api.Middleware;
using MicroserviceB.Api.Telemetry;
using OpenTelemetry.Resources;

namespace MicroserviceB.Api
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
                                 serviceName: "MicroserviceB.Api",
                                 serviceNamespace: telemetryNamespace))
                                .WithTracing(tracing => tracing.AddSource(DemoTelemetry.Name))
                                .WithMetrics(metrics => metrics.AddMeter(DemoTelemetry.Name)); ;

            if (!string.IsNullOrEmpty(builder.Configuration["AzureMonitor:ConnectionString"]))
            {
                otel.UseAzureMonitor();
            }

            // How badly should this service behave? (appsettings.json → "Chaos")
            builder.Services.AddOptions<ChaosOptions>()
                .Bind(builder.Configuration.GetSection(ChaosOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Global exception handler + ProblemDetails for consistent error responses
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();   // GET /openapi/v1.json
            }

            // The exception handler goes first, so it catches anything thrown further down the pipeline
            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
