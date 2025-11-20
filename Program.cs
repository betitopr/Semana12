using Hangfire;
using Hangfire.MySql;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// --- Servicios básicos ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --- Documentación Swagger (opcional, pero útil) ---
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Laboratorio12 API", Version = "v1" });
});

// --- Configurar Hangfire con MySQL ---
var connectionString = $"Server={Environment.GetEnvironmentVariable("MYSQL_HOST")};" +
                       $"Port=3306;" +
                       $"Database={Environment.GetEnvironmentVariable("MYSQL_DATABASE")};" +
                       $"User ID={Environment.GetEnvironmentVariable("MYSQL_USER")};" +
                       $"Password={Environment.GetEnvironmentVariable("MYSQL_PASSWORD")};" +
                       $"Allow User Variables=True";

GlobalConfiguration.Configuration.UseStorage(new MySqlStorage(connectionString));

// --- Servidor de ejecución de Hangfire ---
builder.Services.AddHangfireServer();

// --- Registrar tus servicios personalizados ---
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<DataCleanupService>();

var app = builder.Build();

// --- Configurar pipeline HTTP ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Solo redirigir HTTPS en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// --- Dashboard de Hangfire ---
app.UseHangfireDashboard("/hangfire");

// --- Jobs recurrentes ---

// Job recurrente de notificaciones (diario a medianoche)
RecurringJob.AddOrUpdate<NotificationService>(
    "job-notificacion-diaria",
    x => x.SendNotification("usuario_diario"),
    Cron.Daily
);

// Job recurrente de limpieza de datos (cada hora para demostración)
// En producción, podrías usar Cron.Weekly() o Cron.Monthly()
RecurringJob.AddOrUpdate<DataCleanupService>(
    "job-limpieza-datos",
    x => x.CleanupOldData(),
    Cron.Hourly, // Cambiar a Cron.Weekly() o Cron.Monthly() en producción
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    }
);

app.MapControllers();

// Endpoint de salud simple
app.MapGet("/", () => new { 
    status = "running", 
    message = "Laboratorio12 API está funcionando",
    hangfire = "/hangfire",
    swagger = "/swagger"
}).WithName("HealthCheck");

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Aplicación iniciando en http://localhost:5252");
logger.LogInformation("Dashboard Hangfire disponible en http://localhost:5252/hangfire");
logger.LogInformation("Swagger disponible en http://localhost:5252/swagger");

app.Run();
