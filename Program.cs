using Hangfire;
using Hangfire.MySql;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Web; // Necesario para HttpUtility.ParseQueryString

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

// --- Configurar MySQL Connection String (Lógica Corregida) ---
string connectionString;

// Intentar leer DATABASE_URL primero (Railway/Render style)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL") 
                  ?? builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrEmpty(databaseUrl) && databaseUrl.StartsWith("mysql://"))
{
    // 1. Parsear URL: mysql://user:password@host:port/database?param1=value1&param2=value2
    if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri))
    {
        throw new InvalidOperationException($"DATABASE_URL no es un URI válido: {databaseUrl}");
    }
    
    var userInfo = uri.UserInfo.Split(':');
    
    // 2. Construir la cadena de conexión tradicional
    var connectionStringBuilder = new StringBuilder();

    connectionStringBuilder.Append($"Server={uri.Host};");
    connectionStringBuilder.Append($"Port={uri.Port};");
    connectionStringBuilder.Append($"Database={uri.AbsolutePath.TrimStart('/')};");
    connectionStringBuilder.Append($"User ID={userInfo[0]};");
    connectionStringBuilder.Append($"Password={userInfo[1]};");
    
    // 3. Procesar la Query String y añadir parámetros dinámicamente
    // Esto asegura que "AllowPublicKeyRetrieval=True" se incluya.
    if (!string.IsNullOrEmpty(uri.Query))
    {
        var queryParams = HttpUtility.ParseQueryString(uri.Query);
        
        foreach (string key in queryParams.AllKeys)
        {
            if (!string.IsNullOrEmpty(key))
            {
                // Agrega el par clave-valor al string builder
                connectionStringBuilder.Append($"{key}={queryParams[key]};");
            }
        }
    }
    
    // 4. Añadir parámetros por defecto si no están ya incluidos
    if (!connectionStringBuilder.ToString().Contains("Allow User Variables"))
    {
        connectionStringBuilder.Append("Allow User Variables=True;");
    }
    if (!connectionStringBuilder.ToString().Contains("SslMode"))
    {
        // Nota: Si AllowPublicKeyRetrieval=True falla, puedes probar SslMode=Required (si usas SSL) o SslMode=None (como fallback)
        connectionStringBuilder.Append("SslMode=None;"); 
    }

    connectionString = connectionStringBuilder.ToString();
    
    Console.WriteLine($"✅ Conectando a Railway: {uri.Host}:{uri.Port}/{uri.AbsolutePath.TrimStart('/')}");
}
else
{
    // Fallback a variables individuales (desarrollo local)
    // Se mantiene tu lógica de fallback original
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

// Configuración de trabajos recurrentes
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