using Asp.Versioning.Builder;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();
builder.Services.AddProblemDetails();

var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

app.UseStatusCodePages();

app.MapCatalogApi();

app.UseDefaultOpenApi();
app.Run();
