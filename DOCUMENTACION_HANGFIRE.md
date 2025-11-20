# Documentación: Implementación de Hangfire

## Resumen de la Implementación

Este proyecto implementa Hangfire para la ejecución de trabajos en segundo plano (background jobs) en una aplicación ASP.NET Core.

## Configuración Realizada

### 1. Paquetes NuGet Instalados
- **Hangfire** (v1.8.21): Paquete principal de Hangfire
- **Hangfire.SqlServer** (v1.8.21): Almacenamiento en SQL Server
- **Hangfire.AspNetCore** (v1.8.21): Integración con ASP.NET Core

### 2. Configuración en Program.cs

```csharp
// Configuración de Hangfire con SQL Server Storage
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Middleware para dashboard
app.UseHangfireDashboard("/hangfire");
```

### 3. Cadena de Conexión

Configurada en `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HangfireDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

## Tipos de Jobs Implementados

### 1. Fire-and-Forget (Inmediato)
**Ubicación:** `NotificationController.cs`

```csharp
BackgroundJob.Enqueue(() => new NotificationService().SendNotification("usuario1"));
```

**Características:**
- Se ejecuta inmediatamente después de ser encolado
- No retorna valor
- No se puede cancelar una vez encolado
- Ideal para tareas que no requieren seguimiento

**Endpoint:** `POST /api/notification/fire-and-forget`

### 2. Delayed (Diferido)
**Ubicación:** `NotificationController.cs`

```csharp
BackgroundJob.Schedule(() => new NotificationService().SendNotification("usuario2"), TimeSpan.FromMinutes(10));
```

**Características:**
- Se ejecuta después de un tiempo determinado
- Útil para tareas programadas en el futuro
- Se puede cancelar antes de su ejecución

**Endpoint:** `POST /api/notification/delayed`

### 3. Recurring (Recurrente)
**Ubicación:** `Program.cs` y `NotificationController.cs`

```csharp
RecurringJob.AddOrUpdate("job-notificacion-diaria",
    () => new NotificationService().SendNotification("usuario_diario"), 
    Cron.Daily);
```

**Características:**
- Se ejecuta periódicamente según una expresión Cron
- Se configura automáticamente al iniciar la aplicación
- Puede ser actualizado dinámicamente
- Ejemplos de expresiones Cron:
  - `Cron.Daily` - Diariamente a medianoche
  - `Cron.Hourly` - Cada hora
  - `Cron.Weekly` - Semanalmente
  - `Cron.Monthly` - Mensualmente

**Endpoint:** `POST /api/notification/recurring`

## Servicio de Notificaciones

**Archivo:** `NotificationService.cs`

```csharp
public class NotificationService
{
    public void SendNotification(string user)
    {
        Console.WriteLine($"Notificación enviada a {user} en {DateTime.Now}");
    }
}
```

## Cómo Ejecutar y Monitorear

### 1. Ejecutar la Aplicación

```bash
dotnet run
```

La aplicación se ejecutará en:
- **HTTP:** `http://localhost:5252`
- **HTTPS:** `https://localhost:7095`

### 2. Acceder al Dashboard de Hangfire

Navega a: **http://localhost:5252/hangfire** (o el puerto configurado)

### 3. Probar los Endpoints

#### Fire-and-Forget Job
```bash
POST http://localhost:5252/api/notification/fire-and-forget
```

#### Delayed Job
```bash
POST http://localhost:5252/api/notification/delayed
```

#### Recurring Job
```bash
POST http://localhost:5252/api/notification/recurring
```

### 4. Monitoreo en el Dashboard

El dashboard de Hangfire proporciona:

#### Pestaña "Jobs"
- **Enqueued:** Jobs en cola esperando ejecución
- **Processing:** Jobs actualmente en ejecución
- **Succeeded:** Jobs completados exitosamente
- **Failed:** Jobs que fallaron
- **Scheduled:** Jobs programados para ejecución futura
- **Recurring:** Jobs recurrentes configurados

#### Información Detallada de cada Job
- **Job ID:** Identificador único del job
- **Estado:** Estado actual (Enqueued, Processing, Succeeded, Failed)
- **Método:** Método que se ejecuta
- **Argumentos:** Parámetros pasados al método
- **Fecha de creación:** Cuándo fue creado
- **Fecha de ejecución:** Cuándo se ejecutó
- **Duración:** Tiempo que tardó en ejecutarse
- **Reintentos:** Número de intentos realizados

#### Historial
- Registro completo de todos los jobs ejecutados
- Filtros por estado, fecha, método, etc.
- Detalles de errores si los hay

#### Reintentos
- Hangfire automáticamente reintenta jobs fallidos
- Configurable el número máximo de reintentos
- Intervalo entre reintentos configurable

## Estructura del Proyecto

```
Laboratorio12_Coaquira/
├── Controllers/
│   └── NotificationController.cs    # Endpoints para crear jobs
├── NotificationService.cs           # Servicio de notificaciones
├── Program.cs                       # Configuración de Hangfire
├── appsettings.json                 # Cadena de conexión
└── DOCUMENTACION_HANGFIRE.md        # Esta documentación
```

## Observaciones Importantes

1. **Base de Datos:** Hangfire crea automáticamente las tablas necesarias en la base de datos especificada en la cadena de conexión.

2. **Persistencia:** Todos los jobs se almacenan en SQL Server, por lo que sobreviven a reinicios de la aplicación.

3. **Dashboard:** El dashboard es accesible sin autenticación por defecto. En producción, se recomienda agregar autenticación.

4. **Job Recurrente:** El job recurrente se configura automáticamente al iniciar la aplicación y se ejecutará diariamente a medianoche.

5. **Monitoreo en Tiempo Real:** El dashboard se actualiza automáticamente mostrando el estado actual de los jobs.

## Código Completo

### Program.cs
```csharp
using Hangfire;
using Hangfire.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Configurar Hangfire con SQL Server Storage
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Registrar el servicio de notificaciones
builder.Services.AddScoped<NotificationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Middleware para dashboard de Hangfire
app.UseHangfireDashboard("/hangfire");

// Paso 6: Crear job recurrente
RecurringJob.AddOrUpdate("job-notificacion-diaria",
    () => new NotificationService().SendNotification("usuario_diario"), 
    Cron.Daily);

app.MapControllers();

app.Run();
```

### NotificationController.cs
```csharp
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace Laboratorio12_Coaquira.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    // Paso 4: Encolar job Fire-and-forget
    [HttpPost("fire-and-forget")]
    public IActionResult EnqueueFireAndForgetJob()
    {
        BackgroundJob.Enqueue(() => new NotificationService().SendNotification("usuario1"));
        return Ok(new { message = "Job Fire-and-forget encolado exitosamente" });
    }

    // Paso 5: Crear job delayed (diferido)
    [HttpPost("delayed")]
    public IActionResult ScheduleDelayedJob()
    {
        BackgroundJob.Schedule(() => new NotificationService().SendNotification("usuario2"), TimeSpan.FromMinutes(10));
        return Ok(new { message = "Job Delayed programado para ejecutarse en 10 minutos" });
    }

    // Paso 6: Crear job recurrente
    [HttpPost("recurring")]
    public IActionResult AddOrUpdateRecurringJob()
    {
        RecurringJob.AddOrUpdate("job-notificacion-diaria",
            () => new NotificationService().SendNotification("usuario_diario"), 
            Cron.Daily);
        return Ok(new { message = "Job Recurrente configurado para ejecutarse diariamente" });
    }
}
```

### NotificationService.cs
```csharp
public class NotificationService
{
    public void SendNotification(string user)
    {
        Console.WriteLine($"Notificación enviada a {user} en {DateTime.Now}");
    }
}
```

## Conclusión

Hangfire proporciona una solución robusta y fácil de usar para la ejecución de trabajos en segundo plano. El dashboard permite monitorear en tiempo real el estado de todos los jobs, su historial y manejar errores de manera eficiente.

