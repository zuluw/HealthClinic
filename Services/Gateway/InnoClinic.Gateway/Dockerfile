FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Gateway/InnoClinic.Gateway/InnoClinic.Gateway.csproj", "InnoClinic.Gateway/"]

COPY ["Common/nuget.config", "./"]
COPY ["Common/NugetRepository/", "./Common/NugetRepository/"]

RUN dotnet restore "InnoClinic.Gateway/InnoClinic.Gateway.csproj"

COPY . .
WORKDIR "/src/Gateway/InnoClinic.Gateway"
RUN dotnet build "InnoClinic.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InnoClinic.Gateway.dll"]