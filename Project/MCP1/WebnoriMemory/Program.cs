using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.NET.Server;
using WebnoriMemory.Repositories;
using WebnoriMemory.Services;

var builder = Host.CreateApplicationBuilder(args);

var isServerMode = args.Contains("--serverMode");

if (isServerMode)
{
    builder.Services.AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly(typeof(Program).Assembly);
}

builder.Services.AddSingleton<RavenDbService>();
builder.Services.AddSingleton<WebnoriRepository>();
builder.Services.AddSingleton<VectorService>();
builder.Services.AddSingleton<GeocodingService>();

var host = builder.Build();

if (isServerMode)
{
    await host.RunAsync();
}
else
{
    Console.WriteLine("WebnoriMemory MCP Server");
    Console.WriteLine("Run with --serverMode to start in MCP server mode");
}
