FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Restore separately so source-only changes can reuse the dependency layer.
COPY Common/Common.csproj Common/
COPY DataAccess.Common/DataAccess.Common.csproj DataAccess.Common/
COPY DataAcess/DataAcess.csproj DataAcess/
COPY Service/Service.csproj Service/
COPY API/API.csproj API/
RUN dotnet restore API/API.csproj

COPY . .
RUN dotnet publish API/API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8088
EXPOSE 8088

COPY --from=build /app/publish .

# Force Workstation GC to minimize container RAM footprint
ENV DOTNET_gcServer=0

# Limit the .NET GC Heap to 80% of the Docker container's allocated memory limit
ENV DOTNET_GCHeapHardLimitPercent=80

USER $APP_UID
ENTRYPOINT ["dotnet", "API.dll"]