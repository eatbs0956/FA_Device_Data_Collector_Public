FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CollectorWorker/CollectorWorker.csproj", "CollectorWorker/"]
RUN dotnet restore "CollectorWorker/CollectorWorker.csproj"
COPY . .
WORKDIR "/src/CollectorWorker"
RUN dotnet publish "CollectorWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CollectorWorker.dll"]
