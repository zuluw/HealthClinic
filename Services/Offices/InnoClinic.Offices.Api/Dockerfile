FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["InnoClinic.Offices.Api/InnoClinic.Offices.Api.csproj", "InnoClinic.Offices.Api/"]
COPY ["InnoClinic.Offices.Application/InnoClinic.Offices.Application.csproj", "InnoClinic.Offices.Application/"]
COPY ["InnoClinic.Offices.Domain/InnoClinic.Offices.Domain.csproj", "InnoClinic.Offices.Domain/"]
COPY ["InnoClinic.Offices.Infrastructure/InnoClinic.Offices.Infrastructure.csproj", "InnoClinic.Offices.Infrastructure/"]

RUN dotnet restore "InnoClinic.Offices.Api/InnoClinic.Offices.Api.csproj"

COPY . .
WORKDIR "/src/InnoClinic.Offices.Api"
RUN dotnet build "InnoClinic.Offices.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InnoClinic.Offices.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "InnoClinic.Offices.Api.dll"]