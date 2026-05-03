using Azure.Storage.Blobs;
using InnoClinic.Documents.Api.Middleware;
using InnoClinic.Documents.Application.Interfaces;
using InnoClinic.Documents.Application.Mappings;
using InnoClinic.Documents.Application.Services;
using InnoClinic.Documents.Infrastructure.Blob;
using InnoClinic.Documents.Infrastructure.Data;
using InnoClinic.Documents.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using FluentValidation;
using InnoClinic.Documents.Application.Validators;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var blobConnectionString = builder.Configuration["BlobStorage:ConnectionString"];

builder.Services.AddSingleton(x => new BlobServiceClient(blobConnectionString));
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IBlobService, AzureBlobService>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddSingleton<DocumentMapper>();
builder.Services.AddValidatorsFromAssemblyContaining<DocumentValidator>();

builder.Services.AddMassTransit(x =>
{
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