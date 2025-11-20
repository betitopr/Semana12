public class NotificationService
{
    public void SendNotification(string user)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var message = $"🔔 Notificación enviada a {user} en {timestamp}";
        
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine($"  {message}");
        Console.WriteLine("=".PadRight(60, '='));
        
        // Simular trabajo (puedes agregar lógica real aquí)
        Thread.Sleep(500); // Simula procesamiento
        
        Console.WriteLine($"✅ Job completado exitosamente para {user}");
    }

    /// <summary>
    /// Paso 8: Método que simula un fallo para verificar reintentos automáticos de Hangfire
    /// </summary>
    public void SendNotificationWithFailure(string user)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var attempt = GetAttemptNumber();
        
        Console.WriteLine("\n" + "=".PadRight(70, '='));
        Console.WriteLine($"  ⚠️  INTENTO #{attempt} - Enviando notificación a {user}");
        Console.WriteLine($"  Fecha/Hora: {timestamp}");
        Console.WriteLine("=".PadRight(70, '='));

        // Simular fallo en los primeros 2 intentos
        if (attempt <= 2)
        {
            Console.WriteLine($"\n❌ FALLO SIMULADO en intento #{attempt}");
            Console.WriteLine($"   Razón: Error de conexión al servicio de notificaciones");
            Console.WriteLine($"   Hangfire reintentará automáticamente...\n");
            
            throw new InvalidOperationException(
                $"Error simulado en intento #{attempt}: No se pudo conectar al servicio de notificaciones para {user}"
            );
        }

        // En el tercer intento, tener éxito
        Console.WriteLine($"\n✅ ÉXITO en intento #{attempt}");
        Console.WriteLine($"   Notificación enviada correctamente a {user}");
        Console.WriteLine($"   Mensaje: 'Hola {user}, esta es tu notificación importante'");
        Console.WriteLine("\n" + "=".PadRight(70, '='));
        Console.WriteLine($"  ✅ JOB COMPLETADO EXITOSAMENTE");
        Console.WriteLine("=".PadRight(70, '=') + "\n");
    }

    /// <summary>
    /// Obtiene el número de intento actual (simulado usando un contador estático)
    /// En producción, Hangfire proporciona esta información automáticamente
    /// </summary>
    private static int _attemptCounter = 0;
    private int GetAttemptNumber()
    {
        _attemptCounter++;
        // Resetear después de 3 intentos para simular múltiples ejecuciones
        if (_attemptCounter > 3)
        {
            _attemptCounter = 1;
        }
        return _attemptCounter;
    }
}

