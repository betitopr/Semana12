public class DataCleanupService
{
    // Simulación de datos antiguos en memoria
    private static readonly List<DataRecord> _dataRecords = new();
    private static int _executionCount = 0;

    public class DataRecord
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// Inicializa datos de ejemplo para simular una base de datos
    /// </summary>
    public void InitializeSampleData()
    {
        if (_dataRecords.Count > 0) return; // Ya inicializado

        var random = new Random();
        var now = DateTime.Now;

        for (int i = 1; i <= 50; i++)
        {
            _dataRecords.Add(new DataRecord
            {
                Id = i,
                Name = $"Registro_{i}",
                CreatedAt = now.AddDays(-random.Next(1, 90)), // Datos de 1 a 90 días atrás
                IsDeleted = false
            });
        }

        Console.WriteLine($"📊 Datos de ejemplo inicializados: {_dataRecords.Count} registros");
    }

    /// <summary>
    /// Limpia datos antiguos (más de 30 días) y genera un reporte
    /// </summary>
    public void CleanupOldData()
    {
        _executionCount++;
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var cutoffDate = DateTime.Now.AddDays(-30); // Eliminar datos de más de 30 días

        Console.WriteLine("\n" + "=".PadRight(70, '='));
        Console.WriteLine($"  🧹 INICIANDO LIMPIEZA DE DATOS - Ejecución #{_executionCount}");
        Console.WriteLine($"  Fecha/Hora: {timestamp}");
        Console.WriteLine("=".PadRight(70, '='));

        // Inicializar datos si es la primera vez
        if (_dataRecords.Count == 0)
        {
            InitializeSampleData();
        }

        // Contar registros antes de la limpieza
        var totalBefore = _dataRecords.Count;
        var oldRecords = _dataRecords.Where(r => r.CreatedAt < cutoffDate && !r.IsDeleted).ToList();
        var oldCount = oldRecords.Count;

        Console.WriteLine($"\n📈 ESTADO INICIAL:");
        Console.WriteLine($"   • Total de registros: {totalBefore}");
        Console.WriteLine($"   • Registros antiguos (>30 días): {oldCount}");
        Console.WriteLine($"   • Fecha de corte: {cutoffDate:yyyy-MM-dd}");

        // Simular limpieza
        if (oldCount > 0)
        {
            foreach (var record in oldRecords)
            {
                record.IsDeleted = true;
                Thread.Sleep(50); // Simular tiempo de procesamiento
            }

            var totalAfter = _dataRecords.Count(r => !r.IsDeleted);
            var deletedCount = oldCount;

            Console.WriteLine($"\n🗑️  LIMPIEZA COMPLETADA:");
            Console.WriteLine($"   • Registros eliminados: {deletedCount}");
            Console.WriteLine($"   • Registros restantes: {totalAfter}");
            Console.WriteLine($"   • Espacio liberado: ~{deletedCount * 1024} KB (simulado)");

            // Generar reporte
            GenerateCleanupReport(timestamp, totalBefore, oldCount, deletedCount, totalAfter);
        }
        else
        {
            Console.WriteLine($"\n✅ No hay datos antiguos para limpiar. Todo está actualizado.");
            GenerateCleanupReport(timestamp, totalBefore, 0, 0, totalBefore);
        }

        Console.WriteLine("\n" + "=".PadRight(70, '='));
        Console.WriteLine($"  ✅ LIMPIEZA FINALIZADA EXITOSAMENTE");
        Console.WriteLine("=".PadRight(70, '=') + "\n");
    }

    /// <summary>
    /// Genera un reporte de la limpieza realizada
    /// </summary>
    private void GenerateCleanupReport(string timestamp, int totalBefore, int oldCount, int deletedCount, int totalAfter)
    {
        Console.WriteLine($"\n📄 REPORTE DE LIMPIEZA:");
        Console.WriteLine($"   ┌─────────────────────────────────────────┐");
        Console.WriteLine($"   │ Reporte generado: {timestamp} │");
        Console.WriteLine($"   ├─────────────────────────────────────────┤");
        Console.WriteLine($"   │ Registros antes:     {totalBefore,10} │");
        Console.WriteLine($"   │ Registros antiguos:  {oldCount,10} │");
        Console.WriteLine($"   │ Registros eliminados: {deletedCount,10} │");
        Console.WriteLine($"   │ Registros después:   {totalAfter,10} │");
        Console.WriteLine($"   │ Reducción:           {((double)deletedCount / totalBefore * 100):F1}%        │");
        Console.WriteLine($"   └─────────────────────────────────────────┘");
    }

    /// <summary>
    /// Obtiene estadísticas actuales de los datos
    /// </summary>
    public object GetStatistics()
    {
        if (_dataRecords.Count == 0)
        {
            InitializeSampleData();
        }

        var active = _dataRecords.Count(r => !r.IsDeleted);
        var deleted = _dataRecords.Count(r => r.IsDeleted);
        var old = _dataRecords.Count(r => r.CreatedAt < DateTime.Now.AddDays(-30) && !r.IsDeleted);

        return new
        {
            total = _dataRecords.Count,
            active,
            deleted,
            oldRecords = old,
            lastCleanup = _executionCount > 0 ? $"Ejecución #{_executionCount}" : "Nunca ejecutado"
        };
    }

    /// <summary>
    /// Versión que simula un fallo para probar reintentos
    /// </summary>
    public void CleanupOldDataWithFailure()
    {
        var random = new Random();
        var shouldFail = random.Next(1, 4) <= 2; // 66% de probabilidad de fallar

        Console.WriteLine("\n" + "=".PadRight(70, '='));
        Console.WriteLine($"  ⚠️  SIMULANDO LIMPIEZA CON POSIBLE FALLO");
        Console.WriteLine("=".PadRight(70, '='));

        if (shouldFail)
        {
            Console.WriteLine($"\n❌ ERROR SIMULADO: No se pudo conectar a la base de datos");
            Console.WriteLine($"   Stack trace simulado:");
            Console.WriteLine($"   at DataCleanupService.CleanupOldDataWithFailure()");
            Console.WriteLine($"   at System.Threading.Tasks.Task.Execute()\n");
            throw new Exception("Error simulado: Fallo en la conexión a la base de datos durante la limpieza");
        }

        // Si no falla, ejecutar limpieza normal
        CleanupOldData();
    }
}

