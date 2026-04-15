FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Appointments/InnoClinic.Appointments.Api/InnoClinic.Appointments.Api.csproj", "InnoClinic.Appointments.Api/"]
COPY ["Appointments/InnoClinic.Appointments.Application/InnoClinic.Appointments.Application.csproj", "InnoClinic.Appointments.Application/"]
COPY ["Appointments/InnoClinic.Appointments.Domain/InnoClinic.Appointments.Domain.csproj", "InnoClinic.Appointments.Domain/"]
COPY ["Appointments/InnoClinic.Appointments.Infrastructure/InnoClinic.Appointments.Infrastructure.csproj", "InnoClinic.Appointments.Infrastructure/"]

COPY ["Common/nuget.config", "./"]
COPY ["Common/NugetRepository/", "./Common/NugetRepository/"]

RUN dotnet restore "InnoClinic.Appointments.Api/InnoClinic.Appointments.Api.csproj"

COPY . .
WORKDIR "/src/Appointments/InnoClinic.Appointments.Api"
RUN dotnet build "InnoClinic.Appointments.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Appointments.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InnoClinic.Appointments.Api.dll"]