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

    // Paso 8: Simular fallo para verificar reintentos automáticos
    [HttpPost("simulate-failure")]
    public IActionResult SimulateFailure()
    {
        BackgroundJob.Enqueue(() => new NotificationService().SendNotificationWithFailure("usuario_test"));
        return Ok(new { 
            message = "Job con simulación de fallo encolado. Hangfire reintentará automáticamente si falla.",
            note = "Observa en el dashboard cómo Hangfire reintenta el job automáticamente"
        });
    }

    // Actividad adicional: Limpieza de datos manual
    [HttpPost("cleanup-data")]
    public IActionResult TriggerDataCleanup()
    {
        BackgroundJob.Enqueue(() => new DataCleanupService().CleanupOldData());
        return Ok(new { message = "Job de limpieza de datos encolado exitosamente" });
    }

    // Actividad adicional: Limpieza de datos con posible fallo
    [HttpPost("cleanup-data-with-failure")]
    public IActionResult TriggerDataCleanupWithFailure()
    {
        BackgroundJob.Enqueue(() => new DataCleanupService().CleanupOldDataWithFailure());
        return Ok(new { 
            message = "Job de limpieza con posible fallo encolado",
            note = "Este job tiene 66% de probabilidad de fallar para demostrar reintentos"
        });
    }

    // Actividad adicional: Obtener estadísticas de datos
    [HttpGet("data-statistics")]
    public IActionResult GetDataStatistics()
    {
        var stats = new DataCleanupService().GetStatistics();
        return Ok(stats);
    }
}

