using InnoClinic.Notifications.Application.Interfaces;
using InnoClinic.Notifications.Application.Services;
using InnoClinic.Notifications.Application.Consumers;
using InnoClinic.Notifications.Infrastructure.Data;
using InnoClinic.Notifications.Infrastructure.ExternalServices;
using InnoClinic.Notifications.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<AppointmentCreatedConsumer>();
            x.AddConsumer<ResultCreatedConsumer>();
            x.AddConsumer<ResultUpdatedConsumer>();

            x.UsingAzureServiceBus((context, cfg) =>
            {
                var connectionString = configuration.GetConnectionString("AzureServiceBus");
                cfg.Host(connectionString);
                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

host.Run();