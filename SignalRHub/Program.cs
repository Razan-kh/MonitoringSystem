using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register SignalR service
builder.Services.AddSignalR();

var app = builder.Build();

// Map your SignalR hub endpoint
app.MapHub<AlertsHub>("/alertsHub");

app.Run();