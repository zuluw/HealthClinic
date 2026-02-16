FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["InnoClinic.Profiles.Api/InnoClinic.Profiles.Api.csproj", "InnoClinic.Profiles.Api/"]
COPY ["InnoClinic.Profiles.Application/InnoClinic.Profiles.Application.csproj", "InnoClinic.Profiles.Application/"]
COPY ["InnoClinic.Profiles.Domain/InnoClinic.Profiles.Domain.csproj", "InnoClinic.Profiles.Domain/"]
COPY ["InnoClinic.Profiles.Infrastructure/InnoClinic.Profiles.Infrastructure.csproj", "InnoClinic.Profiles.Infrastructure/"]

RUN dotnet restore "InnoClinic.Profiles.Api/InnoClinic.Profiles.Api.csproj"

COPY . .
WORKDIR "/src/InnoClinic.Profiles.Api"
RUN dotnet build "InnoClinic.Profiles.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Profiles.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InnoClinic.Profiles.Api.dll"]