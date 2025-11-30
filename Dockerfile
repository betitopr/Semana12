# Usa la imagen base de .NET para construir la aplicación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia los archivos del proyecto al contenedor
COPY . ./

# Restaura las dependencias y construye la aplicación
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Usa una imagen base más ligera para ejecutar la aplicación
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expone el puerto en el que se ejecutará la aplicación
# Render asignará el puerto dinámicamente
EXPOSE 80
EXPOSE 10000

# Variables de entorno para producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000}

# Comando para iniciar la aplicación
ENTRYPOINT ["dotnet", "Laboratorio12_Coaquira.dll"]