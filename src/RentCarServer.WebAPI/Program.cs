using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.RateLimiting;
using RentCarServer.Application;
using RentCarServer.Application.Services;
using RentCarServer.Infrastructure;
using RentCarServer.WebAPI;
using RentCarServer.WebAPI.Middlewares;
using RentCarServer.WebAPI.Modules;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
using TS.Result;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRateLimiter(cfr =>
{
    cfr.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 100;
        options.QueueLimit = 100;
        options.Window = TimeSpan.FromSeconds(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    cfr.AddFixedWindowLimiter("login-fixed", options =>
    {
        options.PermitLimit = 5;
        options.QueueLimit = 1;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    cfr.AddFixedWindowLimiter("forgot-password-fixed", options =>
    {
        options.PermitLimit = 2;
        options.Window = TimeSpan.FromMinutes(5);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    cfr.AddFixedWindowLimiter("reset-password-fixed", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    cfr.AddFixedWindowLimiter("check-forgot-password-code-fixed", options =>
    {
        options.PermitLimit = 2;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});
builder.Services.AddControllers()
    .AddOData(opt =>
    opt.Select()
    .Filter()
    .Count()
    .Expand()
    .OrderBy()
    .SetMaxTop(null));
builder.Services.AddCors();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ExceptionHandler>().AddProblemDetails();
builder.Services.AddResponseCompression(opt =>
{
    opt.EnableForHttps = true;
});

builder.Services.AddTransient<CheckTokenMiddleware>();
builder.Services.AddHostedService<CheckLoginTokenBackgroundService>();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseCors(c => c
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowAnyOrigin()
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
app.UseResponseCompression();

app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.UseMiddleware<CheckTokenMiddleware>();

app.UseRateLimiter();

app.MapControllers()
    .RequireRateLimiting("fixed")
    .RequireAuthorization();
app.MapAuth();

app.MapGet("/", () => "Hello World!").RequireAuthorization();
//await app.CreateFirstUser();
app.Run();
