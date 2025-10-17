using System.Reflection;
using Billing.Application;
using Billing.Infrastructure;
using Billing.WebApi;
using Billing.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddApplication()
    .AddInfrastructure(configuration)
    .AddPresentation();

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

var app = builder.Build();

app.MapEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.MapControllers();

await app.RunAsync();