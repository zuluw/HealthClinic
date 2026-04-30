FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Notifications/InnoClinic.Notifications.Api/InnoClinic.Notifications.Api.csproj", "InnoClinic.Notifications.Api/"]
COPY ["Notifications/InnoClinic.Notifications.Application/InnoClinic.Notifications.Application.csproj", "InnoClinic.Notifications.Application/"]
COPY ["Notifications/InnoClinic.Notifications.Domain/InnoClinic.Notifications.Domain.csproj", "InnoClinic.Notifications.Domain/"]
COPY ["Notifications/InnoClinic.Notifications.Infrastructure/InnoClinic.Notifications.Infrastructure.csproj", "InnoClinic.Notifications.Infrastructure/"]

COPY ["Common/nuget.config", "./"]
COPY ["Common/NugetRepository/", "./Common/NugetRepository/"]

RUN dotnet restore "InnoClinic.Notifications.Api/InnoClinic.Notifications.Api.csproj"

COPY . .
WORKDIR "/src/Notifications/InnoClinic.Notifications.Api"
RUN dotnet build "InnoClinic.Notifications.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Notifications.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InnoClinic.Notifications.Api.dll"]