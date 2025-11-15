using System.Reflection;
using Gateway.Application;
using Gateway.Infrastructure;
using Gateway.WebApi;
using Gateway.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddApplication();
builder.Services.AddPresentation(configuration);
builder.Services.AddInfrastructure(configuration);

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.MapReverseProxy();

await app.RunAsync();