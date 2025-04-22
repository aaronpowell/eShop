var builder = WebApplication.CreateBuilder(args);

builder.AddApplicationServices();

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<BasketService>();

app.Run();
