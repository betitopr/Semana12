using Hangfire;
using Hangfire.MySql;
using Microsoft.OpenApi.Models;
using System.Data;

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
// Obtener configuración de variables de entorno o appsettings.json
var mysqlHost = builder.Configuration["MYSQL_HOST"] ?? 
                Environment.GetEnvironmentVariable("MYSQL_HOST") ?? 
                builder.Configuration.GetConnectionString("MYSQL_HOST") ?? 
                "localhost";

var mysqlPort = builder.Configuration["MYSQL_PORT"] ?? 
                Environment.GetEnvironmentVariable("MYSQL_PORT") ?? 
                "3306";

var mysqlDatabase = builder.Configuration["MYSQL_DATABASE"] ?? 
                    Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? 
                    builder.Configuration.GetConnectionString("MYSQL_DATABASE") ?? 
                    "hangfiredb";

var mysqlUser = builder.Configuration["MYSQL_USER"] ?? 
                Environment.GetEnvironmentVariable("MYSQL_USER") ?? 
                builder.Configuration.GetConnectionString("MYSQL_USER") ?? 
                "root";

var mysqlPassword = builder.Configuration["MYSQL_PASSWORD"] ?? 
                    Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? 
                    builder.Configuration.GetConnectionString("MYSQL_PASSWORD") ?? 
                    "";

// Construir connection string
var connectionString = $"Server={mysqlHost};" +
                       $"Port={mysqlPort};" +
                       $"Database={mysqlDatabase};" +
                       $"User ID={mysqlUser};" +
                       $"Password={mysqlPassword};" +
                       $"Allow User Variables=True;";

// Configurar Hangfire con MySQL Storage
builder.Services.AddHangfire(config =>
    config.UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
    {
        TablesPrefix = "Hangfire",
        using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        QueuePollInterval = TimeSpan.FromSeconds(15),
        JobExpirationCheckInterval = TimeSpan.FromHours(1),
        CountersAggregateInterval = TimeSpan.FromMinutes(5),
        PrepareSchemaIfNecessary = true,
        DashboardJobListLimit = 50000,
        TransactionTimeout = TimeSpan.FromMinutes(1),
    })));

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
var port = Environment.GetEnvironmentVariable("PORT") ?? "5252";
var url = $"http://0.0.0.0:{port}";

logger.LogInformation("Aplicación iniciando en {Url}", url);
logger.LogInformation("Dashboard Hangfire disponible en {Url}/hangfire", url);
logger.LogInformation("Swagger disponible en {Url}/swagger", url);
logger.LogInformation("MySQL Host: {Host}, Database: {Database}", mysqlHost, mysqlDatabase);

app.Run(url);
