using FluentValidation;
using InnoClinic.Appointments.Api.Middleware;
using InnoClinic.Appointments.Application.Consumers;
using InnoClinic.Appointments.Application.Interfaces;
using InnoClinic.Appointments.Application.Mappings;
using InnoClinic.Appointments.Application.Services;
using InnoClinic.Appointments.Application.Validators;
using InnoClinic.Appointments.Infrastructure.Data;
using InnoClinic.Appointments.Infrastructure.ExternalServices;
using InnoClinic.Appointments.Infrastructure.Repositories;
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

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddValidatorsFromAssemblyContaining<IAppointmentService>();
builder.Services.AddSingleton<AppointmentMapper>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();

builder.Services.AddHttpClient<IDocumentsClient, DocumentsClient>(client =>
{
    client.BaseAddress = new Uri("http://documents-api:8080/");
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(ServiceCreatedConsumer).Assembly);

    x.AddEntityFrameworkOutbox<AppDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        var username = builder.Configuration["RabbitMq:Username"] ?? "guest";
        var password = builder.Configuration["RabbitMq:Password"] ?? "guest";

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
    try
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred during DB migration");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();