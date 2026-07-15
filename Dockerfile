# --- Этап 1: Сборка (.NET 10 SDK) ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1. Копируем файлы проектов (.csproj) для восстановления зависимостей
COPY Planeta.Domain/Planeta.Domain.csproj Planeta.Domain/
COPY Planeta.Application/Planeta.Application.csproj Planeta.Application/
COPY Planeta.Infrastructure/Planeta.Infrastructure.csproj Planeta.Infrastructure/
COPY Planeta_New/Planeta_New.csproj Planeta_New/

# 2. Восстанавливаем зависимости для главного веб-проекта
RUN dotnet restore Planeta_New/Planeta_New.csproj

# 3. Копируем весь оставшийся исходный код всех проектов
COPY . .

# 4. Собираем и публикуем веб-проект в режиме Release
WORKDIR /src/Planeta_New
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# --- Этап 2: Запуск (.NET 10 Runtime) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Настройка портов по умолчанию для .NET 10
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Точка входа — запускаем твой исполняемый файл
ENTRYPOINT ["dotnet", "Planeta_New.dll"]