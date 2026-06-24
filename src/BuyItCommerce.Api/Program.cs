using Asp.Versioning;
using BuyItCommerce.Api.Endpoints;
using BuyItCommerce.Api.ErrorHandling;
using BuyItCommerce.Api.Health;
using BuyItCommerce.Application;
using BuyItCommerce.Infrastructure;
using BuyItCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, configuration) => configuration
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .ReadFrom.Services(services)
    .WriteTo.Console(new RenderedCompactJsonFormatter()));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("SqlServer")!, name: "sqlserver")
    .AddCheck<MongoHealthCheck>("mongodb");

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapOrderEndpoints();
app.MapHealthChecks("/health");

await ApplyMigrationsAsync(app);

await app.RunAsync();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await context.Database.MigrateAsync();
}
