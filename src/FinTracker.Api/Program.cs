using FinTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configura??o do DbContext com PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=fintracker_db;Username=fintracker;Password=fintracker_secret_pass";

builder.Services.AddDbContext<FinTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS para acesso do Flutter Client (Web/Desktop/Mobile)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FinTracker Core API", Version = "v1", Description = "API RESTful para gest?o financeira pessoal e empresarial (MEI)" });
});

var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinTracker API v1"));
}

// Health Check Endpoint
app.MapGet("/api/v1/health", () => Results.Ok(new
{
    status = "healthy",
    version = "1.0.0",
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("HealthCheck")
.WithOpenApi();

app.Run();
