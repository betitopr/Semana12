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

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Laboratorio12 API", Version = "v1" });
});

// --- Configurar MySQL Connection String ---
string connectionString;

// Intentar leer DATABASE_URL primero (Railway/Render style)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") 
                  ?? builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("mysql://"))
{
    // Parsear URL: mysql://user:password@host:port/database
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    
    connectionString = $"Server={uri.Host};" +
                      $"Port={uri.Port};" +
                      $"Database={uri.AbsolutePath.TrimStart('/')};" +
                      $"User ID={userInfo[0]};" +
                      $"Password={userInfo[1]};" +
                      $"Allow User Variables=True;" +
                      $"SslMode=None;";
    
    Console.WriteLine($"✅ Conectando a Railway: {uri.Host}:{uri.Port}/{uri.AbsolutePath.TrimStart('/')}");
}
else
{
    // Fallback a variables individuales (desarrollo local)
    var mysqlHost = Environment.GetEnvironmentVariable("MYSQL_HOST") 
                    ?? builder.Configuration["MYSQL_HOST"] 
                    ?? "localhost";
    var mysqlPort = Environment.GetEnvironmentVariable("MYSQL_PORT") 
                    ?? builder.Configuration["MYSQL_PORT"] 
                    ?? "3306";
    var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQL_DATABASE") 
                        ?? builder.Configuration["MYSQL_DATABASE"] 
                        ?? "hangfiredb";
    var mysqlUser = Environment.GetEnvironmentVariable("MYSQL_USER") 
                    ?? builder.Configuration["MYSQL_USER"] 
                    ?? "root";
    var mysqlPassword = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") 
                        ?? builder.Configuration["MYSQL_PASSWORD"] 
                        ?? "";
    
    connectionString = $"Server={mysqlHost};" +
                       $"Port={mysqlPort};" +
                       $"Database={mysqlDatabase};" +
                       $"User ID={mysqlUser};" +
                       $"Password={mysqlPassword};" +
                       $"Allow User Variables=True;";
    
    Console.WriteLine($"📍 Conectando a MySQL local: {mysqlHost}:{mysqlPort}/{mysqlDatabase}");
}

// Configurar Hangfire con MySQL Storage
builder.Services.AddHangfire(config =>
    config.UseStorage(new MySqlStorage(connectionString, new MySqlStorageOptions
    {
        TablesPrefix = "Hangfire",
        QueuePollInterval = TimeSpan.FromSeconds(15),
        JobExpirationCheckInterval = TimeSpan.FromHours(1),
        CountersAggregateInterval = TimeSpan.FromMinutes(5),
        PrepareSchemaIfNecessary = true,
        DashboardJobListLimit = 50000,
        TransactionTimeout = TimeSpan.FromMinutes(1)
    })));

builder.Services.AddHangfireServer();

builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<DataCleanupService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<NotificationService>(
    "job-notificacion-diaria",
    x => x.SendNotification("usuario_diario"),
    Cron.Daily
);

RecurringJob.AddOrUpdate<DataCleanupService>(
    "job-limpieza-datos",
    x => x.CleanupOldData(),
    Cron.Hourly,
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    }
);

app.MapControllers();

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

app.Run(url);