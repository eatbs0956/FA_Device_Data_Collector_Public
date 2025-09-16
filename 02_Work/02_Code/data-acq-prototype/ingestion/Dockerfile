FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Ingestion.Api/Ingestion.Api.csproj", "Ingestion.Api/"]
RUN dotnet restore "Ingestion.Api/Ingestion.Api.csproj"
COPY . .
WORKDIR "/src/Ingestion.Api"
RUN dotnet publish "Ingestion.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ingestion.Api.dll"]
