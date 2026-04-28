FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Documents/InnoClinic.Documents.Api/InnoClinic.Documents.Api.csproj", "InnoClinic.Documents.Api/"]
COPY ["Documents/InnoClinic.Documents.Application/InnoClinic.Documents.Application.csproj", "InnoClinic.Documents.Application/"]
COPY ["Documents/InnoClinic.Documents.Domain/InnoClinic.Documents.Domain.csproj", "InnoClinic.Documents.Domain/"]
COPY ["Documents/InnoClinic.Documents.Infrastructure/InnoClinic.Documents.Infrastructure.csproj", "InnoClinic.Documents.Infrastructure/"]

COPY ["Common/nuget.config", "./"]
COPY ["Common/NugetRepository/", "./Common/NugetRepository/"]

RUN dotnet restore "InnoClinic.Documents.Api/InnoClinic.Documents.Api.csproj"

COPY . .
WORKDIR "/src/Documents/InnoClinic.Documents.Api"
RUN dotnet build "InnoClinic.Documents.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Documents.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InnoClinic.Documents.Api.dll"]