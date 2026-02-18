using FluentValidation;
using InnoClinic.Profiles.Api.Middleware;
using InnoClinic.Profiles.Application.Interfaces;
using InnoClinic.Profiles.Application.Services;
using InnoClinic.Profiles.Application.Validators; 
using InnoClinic.Profiles.Infrastructure.Data;    
using InnoClinic.Profiles.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ProfilesDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddValidatorsFromAssemblyContaining<DoctorRequestValidator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProfilesDbContext>();
    try
    {
        context.Database.Migrate();
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

// app.UseSerilogRequestLogging(); 
app.MapControllers();
app.Run();