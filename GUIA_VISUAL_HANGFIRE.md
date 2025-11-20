# 🎯 Guía Visual: ¿Qué hace tu aplicación con Hangfire?

## 📋 Resumen: ¿Qué es Hangfire?

**Hangfire** es un sistema de procesamiento de trabajos en segundo plano (background jobs) para .NET. Tu aplicación lo usa para ejecutar tareas de forma asíncrona sin bloquear las peticiones HTTP.

---

## 🚀 ¿Qué hace tu aplicación específicamente?

Tu aplicación implementa **3 tipos de trabajos (jobs)** que ejecutan notificaciones:

### 1. **Job Fire-and-Forget** (Inmediato) 🔥
- **Qué hace:** Ejecuta una notificación **inmediatamente** después de ser creado
- **Cuándo se usa:** Cuando necesitas procesar algo de forma asíncrona sin esperar
- **Ejemplo:** Enviar un email, procesar una imagen, generar un reporte

### 2. **Job Delayed** (Programado) ⏰
- **Qué hace:** Programa una notificación para ejecutarse **después de un tiempo** (10 minutos en tu caso)
- **Cuándo se usa:** Cuando necesitas ejecutar algo en el futuro
- **Ejemplo:** Enviar recordatorios, limpiar datos antiguos, enviar notificaciones después de un evento

### 3. **Job Recurrente** (Periódico) 🔄
- **Qué hace:** Ejecuta una notificación **automáticamente cada día** a medianoche
- **Cuándo se usa:** Para tareas que deben repetirse periódicamente
- **Ejemplo:** Reportes diarios, backups, limpieza de datos, envío de newsletters

---

## 👀 ¿Qué puedes visualizar en el programa?

### 1. **Dashboard de Hangfire** (Interfaz Web) 📊

**URL:** `http://localhost:5252/hangfire`

Este es el **panel principal** donde verás todo. Incluye:

#### **Pestaña "Jobs" (Trabajos)**
Muestra todos los trabajos con sus estados:

- 🟡 **Enqueued** (En cola): Jobs esperando ejecutarse
- 🔵 **Processing** (Procesando): Jobs ejecutándose ahora mismo
- 🟢 **Succeeded** (Exitosos): Jobs completados correctamente
- 🔴 **Failed** (Fallidos): Jobs que tuvieron errores
- 🟠 **Scheduled** (Programados): Jobs programados para el futuro
- 🟣 **Recurring** (Recurrentes): Jobs que se repiten automáticamente

#### **Pestaña "Recurring Jobs"**
Lista todos los trabajos recurrentes configurados:
- ID del job
- Expresión Cron (cuándo se ejecuta)
- Próxima ejecución
- Última ejecución
- Botón "Trigger now" (ejecutar ahora manualmente)

#### **Información de cada Job**
Al hacer clic en un job, verás:
- **Job ID:** Identificador único
- **Method:** Qué método se ejecutó (`NotificationService.SendNotification`)
- **Arguments:** Parámetros (`["usuario1"]`)
- **State:** Estado actual
- **Created At:** Cuándo se creó
- **Timeline:** Historial de cambios de estado

### 2. **Consola de la Aplicación** 💻

Cuando un job se ejecuta, verás mensajes en la consola:

```
============================================================
  🔔 Notificación enviada a usuario1 en 2024-01-15 14:30:25
============================================================
✅ Job completado exitosamente para usuario1
```

### 3. **Swagger UI** (Documentación de API) 📚

**URL:** `http://localhost:5252/swagger`

Aquí puedes:
- Ver todos los endpoints disponibles
- Probar los endpoints directamente desde el navegador
- Ver la documentación de cada endpoint

---

## 🔍 ¿Cómo ver los cambios en tiempo real?

### **Paso 1: Ejecutar la aplicación**

```bash
cd Laboratorio12_Coaquira
dotnet run
```

O desde Rider: Presiona **F5** o haz clic en el botón de ejecutar.

### **Paso 2: Abrir el Dashboard de Hangfire**

1. Abre tu navegador
2. Ve a: `http://localhost:5252/hangfire`
3. **¡El dashboard se actualiza automáticamente cada pocos segundos!**

### **Paso 3: Probar los diferentes tipos de jobs**

#### **Prueba 1: Job Fire-and-Forget (Inmediato)**

**Opción A: Desde Swagger**
1. Ve a `http://localhost:5252/swagger`
2. Busca `POST /api/notification/fire-and-forget`
3. Haz clic en "Try it out" → "Execute"

**Opción B: Desde el archivo .http**
1. Abre `Laboratorio12_Coaquira.http` en Rider
2. Haz clic en el botón ▶️ junto a "2. Crear un Job Fire-and-Forget"

**Opción C: Desde Postman/Thunder Client**
```
POST http://localhost:5252/api/notification/fire-and-forget
```

**¿Qué verás?**
1. ✅ En el dashboard: El job aparece en "Enqueued" → luego "Processing" → luego "Succeeded"
2. ✅ En la consola: Verás el mensaje de notificación
3. ✅ Respuesta HTTP: `{ "message": "Job Fire-and-forget encolado exitosamente" }`

#### **Prueba 2: Job Delayed (Programado)**

**Ejecuta:**
```
POST http://localhost:5252/api/notification/delayed
```

**¿Qué verás?**
1. ✅ En el dashboard: El job aparece en "Scheduled" con la hora de ejecución (10 minutos después)
2. ⏰ Espera 10 minutos (o cambia el tiempo en el código para probar más rápido)
3. ✅ Después de 10 minutos: El job se mueve a "Processing" → "Succeeded"
4. ✅ En la consola: Verás el mensaje cuando se ejecute

**💡 Tip:** Para probar más rápido, cambia `TimeSpan.FromMinutes(10)` a `TimeSpan.FromSeconds(30)` en `NotificationController.cs`

#### **Prueba 3: Job Recurrente (Automático)**

**Ya está configurado automáticamente** cuando inicias la aplicación.

**¿Qué verás?**
1. ✅ En el dashboard → Pestaña "Recurring Jobs": Verás `job-notificacion-diaria`
2. ✅ Se ejecutará automáticamente cada día a las 00:00 (medianoche)
3. ✅ Cada ejecución crea un nuevo job en el historial

**Para ejecutarlo manualmente:**
1. Ve a `http://localhost:5252/hangfire`
2. Pestaña "Recurring Jobs"
3. Haz clic en el botón "Trigger now" (⚡) junto al job
4. Verás cómo se crea y ejecuta inmediatamente

---

## 📈 Flujo Visual de un Job

```
1. CREACIÓN
   ↓
   [API Endpoint] → Crea el job
   ↓
2. ENCOLADO
   ↓
   [Dashboard: "Enqueued"] ← Job esperando
   ↓
3. PROCESAMIENTO
   ↓
   [Dashboard: "Processing"] ← Job ejecutándose
   ↓
   [Consola: Mensaje de notificación] ← Ver aquí
   ↓
4. COMPLETADO
   ↓
   [Dashboard: "Succeeded"] ← Job terminado
   ↓
   [Historial permanente] ← Se guarda en MySQL
```

---

## 🎯 Checklist: ¿Cómo saber que todo funciona?

### ✅ Verificación Inicial

- [ ] La aplicación inicia sin errores
- [ ] Puedes acceder a `http://localhost:5252`
- [ ] Puedes acceder a `http://localhost:5252/hangfire`
- [ ] En "Recurring Jobs" ves `job-notificacion-diaria`

### ✅ Verificación de Jobs

- [ ] Al crear un Fire-and-Forget, aparece en el dashboard
- [ ] El job pasa de "Enqueued" → "Processing" → "Succeeded"
- [ ] Ves el mensaje en la consola
- [ ] Al crear un Delayed, aparece en "Scheduled"
- [ ] El job recurrente está configurado correctamente

### ✅ Verificación de Persistencia

- [ ] Los jobs se guardan en MySQL (base de datos `hangfiredb`)
- [ ] Si reinicias la aplicación, los jobs programados se mantienen
- [ ] El historial de jobs se conserva

---

## 🔧 Cambios que puedes hacer para ver mejor los resultados

### 1. **Cambiar el tiempo del Job Delayed**

En `NotificationController.cs`, línea 22:
```csharp
// Cambiar de 10 minutos a 30 segundos para pruebas
BackgroundJob.Schedule(() => new NotificationService().SendNotification("usuario2"), 
    TimeSpan.FromSeconds(30)); // ← Cambia aquí
```

### 2. **Cambiar la frecuencia del Job Recurrente**

En `Program.cs`, línea 64:
```csharp
// Ejecutar cada minuto en lugar de diariamente (para pruebas)
RecurringJob.AddOrUpdate<NotificationService>(
    "job-notificacion-diaria",
    x => x.SendNotification("usuario_diario"),
    Cron.Minutely); // ← Cambia de Cron.Daily a Cron.Minutely
```

### 3. **Agregar más información al servicio**

Puedes modificar `NotificationService.cs` para hacer algo más visible:
- Enviar un email
- Guardar en una base de datos
- Escribir en un archivo
- Hacer una llamada HTTP

---

## 🎓 Conceptos Clave

### **¿Por qué usar Hangfire?**

- ✅ **No bloquea:** Las peticiones HTTP responden rápido
- ✅ **Confiabilidad:** Los jobs se guardan en la base de datos
- ✅ **Persistencia:** Si la app se cae, los jobs se recuperan
- ✅ **Monitoreo:** Dashboard visual para ver todo
- ✅ **Escalabilidad:** Puedes tener múltiples servidores procesando jobs

### **¿Dónde se guardan los jobs?**

En tu base de datos MySQL (`hangfiredb`). Hangfire crea tablas automáticamente:
- `hangfire.job` - Información de los jobs
- `hangfire.state` - Estados de los jobs
- `hangfire.set` - Conjuntos de jobs (recurrentes, etc.)

---

## 🚨 Solución de Problemas

### **No veo cambios en el dashboard**
- ✅ Asegúrate de que la aplicación esté ejecutándose
- ✅ Refresca el navegador (F5)
- ✅ El dashboard se actualiza automáticamente, pero a veces hay un pequeño retraso

### **Los jobs no se ejecutan**
- ✅ Verifica que MySQL esté corriendo
- ✅ Verifica la conexión en `appsettings.json`
- ✅ Revisa la consola de la aplicación para ver errores

### **No veo mensajes en la consola**
- ✅ Asegúrate de estar viendo la consola correcta (donde ejecutaste `dotnet run`)
- ✅ Los mensajes aparecen cuando el job se ejecuta, no cuando se crea

---

## 📝 Resumen Rápido

1. **Ejecuta la app:** `dotnet run` o F5 en Rider
2. **Abre el dashboard:** `http://localhost:5252/hangfire`
3. **Prueba los endpoints:** Usa Swagger o el archivo .http
4. **Observa los cambios:** 
   - Dashboard se actualiza automáticamente
   - Consola muestra mensajes cuando se ejecutan jobs
   - Los jobs pasan por diferentes estados

**¡Disfruta monitoreando tus jobs en tiempo real! 🎉**



