using FluentValidation;
using InnoClinic.Appointments.Api.Caching;
using InnoClinic.Appointments.Api.Middleware;
using InnoClinic.Appointments.Application.Consumers;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Mappings;
using InnoClinic.Appointments.Application.Services;
using InnoClinic.Appointments.Application.Validators;
using InnoClinic.Appointments.Domain.Models;
using InnoClinic.Appointments.Infrastructure.Data;
using InnoClinic.Appointments.Infrastructure.ExternalServices;
using InnoClinic.Appointments.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var expirationMinutes = builder.Configuration.GetValue<int>("CacheSettings:CatalogExpirationMinutes");

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(5)));
    options.AddPolicy("CatalogPolicy", builder =>
    {
        builder.AddPolicy<CatalogPolicy>();
        builder.Expire(TimeSpan.FromMinutes(expirationMinutes));
    });
});

builder.Services.AddSingleton<CatalogPolicy>();

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Appointments_";
});

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddValidatorsFromAssemblyContaining<IAppointmentService>();
builder.Services.AddSingleton<AppointmentMapper>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();

var documentsApiUrl = builder.Configuration["ExternalServices:DocumentsApiUrl"];

builder.Services.AddHttpClient<IDocumentsClient, DocumentsClient>(client =>
{
    client.BaseAddress = new Uri(documentsApiUrl);
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(ServiceCreatedConsumer).Assembly);

    x.AddEntityFrameworkOutbox<AppDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    var useAzure = builder.Configuration.GetValue<bool>("UseAzureServiceBus");

    if (useAzure)
    {
        x.UsingAzureServiceBus((context, cfg) =>
        {
            var connectionString = builder.Configuration.GetConnectionString("AzureServiceBus");
            cfg.Host(connectionString);
            cfg.ConfigureEndpoints(context);
        });
    }
    else
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            var host = builder.Configuration["RabbitMq:Host"];
            var username = builder.Configuration["RabbitMq:Username"];
            var password = builder.Configuration["RabbitMq:Password"];

            cfg.Host(host, "/", h =>
            {
                h.Username(username);
                h.Password(password);
            });

            cfg.ConfigureEndpoints(context);
        });
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    try
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();

            if (configuration.GetValue<bool>("SeedData"))
            {
                await DbSeeder.SeedAsync(context);
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred during DB migration");
    }
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();