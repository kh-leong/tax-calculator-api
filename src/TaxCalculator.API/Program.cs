using Scalar.AspNetCore;
using TaxCalculator.Application.DependencyInjection;
using TaxCalculator.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => {
        List<ScalarServer> servers = [];
        string? httpsPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT");
        if (httpsPort is not null)
        {
            servers.Add(new ScalarServer($"https://localhost:{httpsPort}"));
        }

        string? httpPort = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORT");
        if (httpPort is not null)
        {
            servers.Add(new ScalarServer($"http://localhost:{httpPort}"));
        }

        options.Servers = servers;
        options.Title = "Tax Calculator API";
        options.ShowSidebar = true;
    });
}

// commented out for simplicity of local testing, but should be enabled in production.
//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
