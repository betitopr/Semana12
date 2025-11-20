# Guía de Monitoreo - Dashboard de Hangfire

## Acceso al Dashboard

**URL:** http://localhost:5252/hangfire

## Qué Observar en el Dashboard

### 1. Panel Principal (Jobs)

Al acceder al dashboard, verás un panel con las siguientes secciones:

#### **Enqueued (En Cola)**
- Muestra los jobs que están esperando ser procesados
- Al crear un job Fire-and-forget, aparecerá aquí primero
- Color: Azul

#### **Processing (En Procesamiento)**
- Jobs que están siendo ejecutados actualmente
- Aparecen aquí cuando Hangfire comienza a procesarlos
- Color: Amarillo/Naranja

#### **Succeeded (Exitosos)**
- Jobs que se completaron correctamente
- Muestra el total de jobs exitosos
- Color: Verde

#### **Failed (Fallidos)**
- Jobs que fallaron durante la ejecución
- Muestra detalles del error
- Color: Rojo

#### **Scheduled (Programados)**
- Jobs que están programados para ejecutarse en el futuro
- Incluye jobs Delayed
- Muestra la fecha/hora de ejecución programada
- Color: Púrpura

#### **Recurring (Recurrentes)**
- Jobs que se ejecutan periódicamente
- Muestra el job "job-notificacion-diaria" configurado
- Muestra la expresión Cron (Daily = diariamente)
- Color: Azul claro

### 2. Detalles de un Job

Al hacer clic en un job, verás:

#### **Información General**
- **Job ID:** Identificador único (ej: `12345678-1234-1234-1234-123456789012`)
- **State:** Estado actual (Enqueued, Processing, Succeeded, Failed, Scheduled)
- **Created At:** Fecha y hora de creación
- **Method:** `NotificationService.SendNotification`
- **Arguments:** `["usuario1"]` o el usuario correspondiente

#### **Historial de Estados**
- Timeline de cambios de estado
- Transiciones: Enqueued → Processing → Succeeded
- Si falla: Enqueued → Processing → Failed

#### **Reintentos (si aplica)**
- Número de intentos realizados
- Razón del fallo (si hubo)
- Stack trace del error (si aplica)

### 3. Pestaña "Recurring Jobs"

Aquí verás:
- **ID del Job:** `job-notificacion-diaria`
- **Cron:** `0 0 * * *` (diariamente a medianoche)
- **Time Zone:** Zona horaria configurada
- **Next Execution:** Próxima fecha/hora de ejecución
- **Last Execution:** Última vez que se ejecutó
- **Last Job ID:** ID del último job ejecutado

### 4. Pestaña "Retries"

Muestra:
- Jobs que están en proceso de reintento
- Número de intentos realizados
- Próximo intento programado

## Flujo de Ejecución Observado

### Job Fire-and-Forget
1. **Creación:** Al llamar `POST /api/notification/fire-and-forget`
2. **Estado Inicial:** Aparece en "Enqueued" (En Cola)
3. **Procesamiento:** Se mueve a "Processing" (En Procesamiento)
4. **Completado:** Se mueve a "Succeeded" (Exitosos)
5. **Tiempo Total:** Generalmente menos de 1 segundo

### Job Delayed
1. **Creación:** Al llamar `POST /api/notification/delayed`
2. **Estado Inicial:** Aparece en "Scheduled" (Programados)
3. **Espera:** Permanece aquí hasta que pasen 10 minutos
4. **Procesamiento:** Se mueve a "Processing" cuando llega el momento
5. **Completado:** Se mueve a "Succeeded"

### Job Recurrente
1. **Configuración:** Se configura automáticamente al iniciar la app
2. **Estado:** Aparece en "Recurring Jobs"
3. **Ejecución:** Se ejecuta diariamente a medianoche (00:00)
4. **Cada Ejecución:** Crea un nuevo job que pasa por el ciclo normal

## Ejemplo de Prueba Paso a Paso

### Paso 1: Verificar el Dashboard Inicial
- Abre http://localhost:5252/hangfire
- Deberías ver el job recurrente "job-notificacion-diaria" en la pestaña "Recurring Jobs"
- Los contadores de jobs deberían estar en 0 (excepto si ya se ejecutó el recurrente)

### Paso 2: Crear un Job Fire-and-Forget
```bash
POST http://localhost:5252/api/notification/fire-and-forget
```

**Observar:**
- El contador de "Enqueued" aumenta a 1
- Inmediatamente se mueve a "Processing"
- Luego aparece en "Succeeded"
- En la consola de la aplicación verás: `Notificación enviada a usuario1 en [fecha/hora]`

### Paso 3: Crear un Job Delayed
```bash
POST http://localhost:5252/api/notification/delayed
```

**Observar:**
- El contador de "Scheduled" aumenta a 1
- El job aparece en la lista de "Scheduled" con la hora de ejecución (10 minutos después)
- Después de 10 minutos, se mueve a "Processing" y luego a "Succeeded"

### Paso 4: Verificar Job Recurrente
- Ve a la pestaña "Recurring Jobs"
- Verás "job-notificacion-diaria" con:
  - Cron: `0 0 * * *`
  - Next Execution: Mañana a las 00:00
  - Puedes hacer clic en "Trigger now" para ejecutarlo manualmente

## Métricas Importantes

### Tiempo de Ejecución
- Los jobs simples (como SendNotification) deberían ejecutarse en milisegundos
- Se muestra en los detalles del job

### Tasa de Éxito
- Revisa la relación entre "Succeeded" y "Failed"
- En un entorno saludable, la mayoría debería estar en "Succeeded"

### Jobs Pendientes
- "Enqueued" + "Scheduled" = Jobs pendientes de ejecución
- Si este número crece constantemente, puede indicar que el servidor está sobrecargado

## Solución de Problemas

### Si no ves el Dashboard
- Verifica que la aplicación esté ejecutándose
- Verifica que la ruta sea correcta: `/hangfire`
- Revisa los logs de la aplicación

### Si los Jobs no se ejecutan
- Verifica la conexión a la base de datos
- Revisa que `AddHangfireServer()` esté configurado
- Verifica los logs de la aplicación para errores

### Si un Job falla
- Haz clic en el job fallido para ver el error
- Revisa el stack trace
- Hangfire reintentará automáticamente según la configuración

## Capturas de Pantalla Sugeridas

Para documentar, captura:
1. Vista general del dashboard mostrando los contadores
2. Detalle de un job Fire-and-forget exitoso
3. Lista de jobs Scheduled (delayed)
4. Pestaña de Recurring Jobs
5. Historial de ejecuciones

## Notas Finales

- El dashboard se actualiza automáticamente cada pocos segundos
- Los datos persisten en la base de datos SQL Server
- Puedes refrescar manualmente la página
- Los jobs recurrentes se ejecutan incluso si la aplicación se reinicia (siempre que esté corriendo a la hora programada)

