using FluentValidation;
using InnoClinic.Offices.Api.Middleware;
using InnoClinic.Offices.Application.Interfaces;
using InnoClinic.Offices.Application.Services;
using InnoClinic.Offices.Application.Validators;
using InnoClinic.Offices.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var requiredSettings = new[] {
    "MongoDbSettings:ConnectionString",
    "MongoDbSettings:DatabaseName"
};

foreach (var key in requiredSettings)
{
    if (string.IsNullOrEmpty(builder.Configuration[key]))
    {
        throw new InvalidOperationException(
            $"Critical configuration error: Parameter '{key}' not found in configuration!");
    }
}

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

MongoDbConfig.Configure();
builder.Services.AddValidatorsFromAssemblyContaining<OfficeValidator>();
builder.Services.AddScoped<IOfficeRepository, OfficeRepository>();
builder.Services.AddScoped<IOfficeService, OfficeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthorization();
app.MapControllers();
app.Run();