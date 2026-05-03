using Hangfire;
using Hangfire.PostgreSql;
using InnoClinic.Notifications.Api.Middleware;
using InnoClinic.Notifications.Application.Consumers;
using InnoClinic.Notifications.Application.Interfaces;
using InnoClinic.Notifications.Application.Services;
using InnoClinic.Notifications.Infrastructure.Data;
using InnoClinic.Notifications.Infrastructure.ExternalServices;
using InnoClinic.Notifications.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AppointmentCreatedConsumer>();
    x.AddConsumer<ResultCreatedConsumer>();
    x.AddConsumer<ResultUpdatedConsumer>();

    x.AddEntityFrameworkOutbox<AppDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

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
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    try
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        var cronExpression = configuration["NotificationSettings:CronSchedule"];
        var jobId = configuration["NotificationSettings:JobId"];

        if (!string.IsNullOrEmpty(cronExpression) && !string.IsNullOrEmpty(jobId))
        {
            recurringJobManager.AddOrUpdate<INotificationService>(
                jobId,
                service => service.SendDailyRemindersAsync(CancellationToken.None),
                cronExpression);

            Log.Information("Hangfire recurring job '{JobId}' registered successfully", jobId);
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred during DB migration");
    }
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = []
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();