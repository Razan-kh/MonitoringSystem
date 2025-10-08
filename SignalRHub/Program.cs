using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var app = builder.Build();

// Get the route from configuration
var hubRoute = builder.Configuration["AlertsHubRoute"];

// Map hub dynamically
app.MapHub<AlertsHub>(hubRoute);

app.Run();