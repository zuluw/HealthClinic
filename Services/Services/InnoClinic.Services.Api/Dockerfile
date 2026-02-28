FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем файлы проектов (пути относительно контекста, который мы укажем в compose)
COPY ["InnoClinic.Services.Api/InnoClinic.Services.Api.csproj", "InnoClinic.Services.Api/"]
COPY ["InnoClinic.Services.Application/InnoClinic.Services.Application.csproj", "InnoClinic.Services.Application/"]
COPY ["InnoClinic.Services.Domain/InnoClinic.Services.Domain.csproj", "InnoClinic.Services.Domain/"]
COPY ["InnoClinic.Services.Infrastructure/InnoClinic.Services.Infrastructure.csproj", "InnoClinic.Services.Infrastructure/"]

RUN dotnet restore "InnoClinic.Services.Api/InnoClinic.Services.Api.csproj"

COPY . .
WORKDIR "/src/InnoClinic.Services.Api"
RUN dotnet build "InnoClinic.Services.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Services.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InnoClinic.Services.Api.dll"]