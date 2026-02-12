# 1. Usamos la imagen oficial de .NET SDK para cocinar (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2. Copiamos los archivos del proyecto y restauramos dependencias
COPY *.csproj ./
RUN dotnet restore

# 3. Copiamos todo lo demás y publicamos la App
COPY . .
RUN dotnet publish -c Release -o /app/out

# 4. Usamos una imagen más ligera solo para servir la App (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# 5. Le decimos que escuche en el puerto 10000 (El de Render)
EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

# 6. ¡Arrancamos! 
# OJO: Si tu archivo de proyecto NO se llama "Task.csproj", cambia "Task.dll" por "TuNombre.dll"
ENTRYPOINT ["dotnet", "MiPrimeraApi.dll"]