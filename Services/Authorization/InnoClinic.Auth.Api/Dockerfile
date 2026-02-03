FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["InnoClinic.Auth.Api/InnoClinic.Auth.Api.csproj", "InnoClinic.Auth.Api/"]
COPY ["InnoClinic.Auth.Application/InnoClinic.Auth.Application.csproj", "InnoClinic.Auth.Application/"]
COPY ["InnoClinic.Auth.Domain/InnoClinic.Auth.Domain.csproj", "InnoClinic.Auth.Domain/"]
COPY ["InnoClinic.Auth.Infrastructure/InnoClinic.Auth.Infrastructure.csproj", "InnoClinic.Auth.Infrastructure/"]

RUN dotnet restore "InnoClinic.Auth.Api/InnoClinic.Auth.Api.csproj"

COPY . .
WORKDIR "/src/InnoClinic.Auth.Api"
RUN dotnet build "InnoClinic.Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Auth.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InnoClinic.Auth.Api.dll"]