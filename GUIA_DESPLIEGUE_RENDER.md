# Guía de Despliegue en Render - Laboratorio 12

Esta guía te ayudará a desplegar tu aplicación .NET con Hangfire y MySQL en Render paso a paso.

## 📋 Requisitos Previos

1. Cuenta en [Render.com](https://render.com)
2. Repositorio en GitHub: `https://github.com/betitopr/Semana12`
3. Proyecto .NET configurado con Hangfire y MySQL

---

## 🚀 Paso 1: Crear Base de Datos MySQL en Render

### 1.1. Crear el Servicio de Base de Datos

1. Inicia sesión en tu cuenta de Render
2. En el dashboard, haz clic en **"New +"** → **"PostgreSQL"** o **"MySQL"**
3. Si no ves MySQL directamente, busca en la lista de servicios disponibles
4. Si Render solo ofrece PostgreSQL, puedes usar PostgreSQL con Hangfire (ver alternativa al final)

**Configuración de la Base de Datos:**
- **Name**: `laboratorio12-db` (o el nombre que prefieras)
- **Database**: `hangfiredb` (o el nombre que prefieras)
- **User**: Se generará automáticamente
- **Region**: Elige la región más cercana a tus usuarios
- **Plan**: Free tier (para desarrollo) o Starter (recomendado para producción)

### 1.2. Obtener las Credenciales de Conexión

Una vez creada la base de datos:

1. Ve a la página de tu base de datos en Render
2. En la sección **"Connections"** o **"Info"**, encontrarás:
   - **Internal Database URL**: Para conexiones desde otros servicios de Render
   - **External Connection String**: Para conexiones externas
   - **Host**: Dirección del servidor
   - **Port**: Puerto (generalmente 3306 para MySQL)
   - **Database**: Nombre de la base de datos
   - **User**: Usuario
   - **Password**: Contraseña

**⚠️ IMPORTANTE**: Guarda estas credenciales, las necesitarás en el siguiente paso.

---

## 🚀 Paso 2: Crear el Servicio Web en Render

### 2.1. Crear Nuevo Servicio Web

1. En el dashboard de Render, haz clic en **"New +"** → **"Web Service"**
2. Conecta tu repositorio de GitHub:
   - Si es la primera vez, autoriza Render para acceder a tu cuenta de GitHub
   - Selecciona el repositorio: `betitopr/Semana12`
   - Selecciona la rama: `main` o `master` (según tu repositorio)

### 2.2. Configurar el Servicio

**Configuración Básica:**
- **Name**: `laboratorio12-api` (o el nombre que prefieras)
- **Region**: Misma región que tu base de datos (para mejor rendimiento)
- **Branch**: `main` o `master`
- **Root Directory**: `Laboratorio12_Coaquira` (si tu proyecto está en una subcarpeta)
- **Runtime**: `.NET`
- **Build Command**: `dotnet restore && dotnet publish -c Release -o ./publish`
- **Start Command**: `dotnet ./publish/Laboratorio12_Coaquira.dll`

**⚠️ NOTA**: Si tu proyecto está en la raíz del repositorio, deja "Root Directory" vacío.

---

## 🔐 Paso 3: Configurar Variables de Entorno

En la sección **"Environment Variables"** del servicio web, agrega las siguientes variables:

### Variables de Base de Datos MySQL

```
MYSQL_HOST=<host-de-tu-base-de-datos>
MYSQL_PORT=3306
MYSQL_DATABASE=<nombre-de-tu-base-de-datos>
MYSQL_USER=<usuario-de-tu-base-de-datos>
MYSQL_PASSWORD=<contraseña-de-tu-base-de-datos>
```

**Ejemplo:**
```
MYSQL_HOST=dpg-xxxxx-a.oregon-postgres.render.com
MYSQL_PORT=3306
MYSQL_DATABASE=hangfiredb
MYSQL_USER=usuario_db
MYSQL_PASSWORD=tu_contraseña_segura
```

### Variables Adicionales (Opcionales)

```
ASPNETCORE_ENVIRONMENT=Production
PORT=10000
```

**⚠️ IMPORTANTE**: 
- Render asigna automáticamente la variable `PORT`, pero puedes configurarla manualmente
- No uses `localhost` para `MYSQL_HOST`, usa la dirección interna o externa proporcionada por Render

---

## 🐳 Paso 4: Verificar Dockerfile (Opcional)

Si Render no detecta automáticamente .NET, puedes usar Docker. Tu `Dockerfile` actual está bien, pero asegúrate de que:

1. El Dockerfile esté en la raíz del proyecto o en la carpeta correcta
2. Si tu proyecto está en una subcarpeta, ajusta las rutas en el Dockerfile

**Si Render usa Docker automáticamente**, no necesitas hacer nada más.

---

## 🚀 Paso 5: Desplegar

1. Haz clic en **"Create Web Service"** o **"Save Changes"**
2. Render comenzará a construir y desplegar tu aplicación
3. Puedes ver el progreso en la pestaña **"Logs"**
4. El despliegue puede tardar 5-10 minutos la primera vez

---

## ✅ Paso 6: Verificar el Despliegue

Una vez completado el despliegue:

1. **Verificar el endpoint raíz:**
   ```
   https://tu-app.onrender.com/
   ```
   Deberías ver: `{"status":"running","message":"Laboratorio12 API está funcionando",...}`

2. **Verificar Hangfire Dashboard:**
   ```
   https://tu-app.onrender.com/hangfire
   ```
   Deberías ver el dashboard de Hangfire con los jobs configurados

3. **Verificar Swagger (si está habilitado):**
   ```
   https://tu-app.onrender.com/swagger
   ```

4. **Verificar los logs:**
   - Ve a la pestaña **"Logs"** en Render
   - Busca mensajes como:
     - "Aplicación iniciando en..."
     - "MySQL Host: ..., Database: ..."
     - Si hay errores de conexión, aparecerán aquí

---

## 🔧 Solución de Problemas Comunes

### Problema 1: Error de Conexión a MySQL

**Síntomas:**
- La aplicación no inicia
- Errores en logs sobre "Unable to connect to MySQL"

**Solución:**
1. Verifica que las variables de entorno estén correctamente configuradas
2. Asegúrate de usar el **Internal Database URL** o **Host interno** si ambos servicios están en Render
3. Verifica que el puerto sea `3306` para MySQL
4. Asegúrate de que la base de datos esté en estado "Available"

### Problema 2: Puerto no Configurado

**Síntomas:**
- Error: "Failed to bind to address"

**Solución:**
- Render asigna automáticamente el puerto, pero asegúrate de que tu código lea la variable `PORT`
- Ya está configurado en tu `Program.cs` actualizado

### Problema 3: Hangfire no Crea las Tablas

**Síntomas:**
- Dashboard de Hangfire vacío o con errores

**Solución:**
- El código actualizado tiene `PrepareSchemaIfNecessary = true`, que crea las tablas automáticamente
- Si persiste, verifica los permisos del usuario de la base de datos

### Problema 4: Build Falla

**Síntomas:**
- Error durante el proceso de build

**Solución:**
1. Verifica que el **Root Directory** esté correcto
2. Verifica que el **Build Command** sea correcto
3. Revisa los logs de build para ver el error específico

---

## 🔄 Actualizar el Código

Si necesitas hacer cambios:

1. Haz commit y push a tu repositorio de GitHub
2. Render detectará automáticamente los cambios y desplegará una nueva versión
3. Puedes ver el progreso en la pestaña **"Events"**

---

## 📝 Resumen de Variables de Entorno Necesarias

```
MYSQL_HOST=<host-de-render>
MYSQL_PORT=3306
MYSQL_DATABASE=<nombre-de-la-base-de-datos>
MYSQL_USER=<usuario>
MYSQL_PASSWORD=<contraseña>
ASPNETCORE_ENVIRONMENT=Production (opcional)
```

---

## 🎯 Checklist Final

- [ ] Base de datos MySQL creada en Render
- [ ] Credenciales de base de datos guardadas
- [ ] Servicio web creado y conectado al repositorio
- [ ] Variables de entorno configuradas correctamente
- [ ] Build completado sin errores
- [ ] Aplicación desplegada y accesible
- [ ] Dashboard de Hangfire funcionando
- [ ] Jobs recurrentes visibles en Hangfire

---

## 💡 Alternativa: Usar PostgreSQL en lugar de MySQL

Si Render solo ofrece PostgreSQL en tu región, puedes:

1. Cambiar el paquete NuGet de `Hangfire.MySqlStorage` a `Hangfire.PostgreSql`
2. Actualizar `Program.cs` para usar PostgreSQL Storage
3. Seguir los mismos pasos pero con una base de datos PostgreSQL

**¿Necesitas ayuda con PostgreSQL?** Puedo ayudarte a migrar el código si es necesario.

---

## 📞 Soporte

Si encuentras problemas:
1. Revisa los logs en Render
2. Verifica que todas las variables de entorno estén configuradas
3. Asegúrate de que la base de datos esté en estado "Available"
4. Verifica que el repositorio esté correctamente conectado

¡Buena suerte con tu despliegue! 🚀
