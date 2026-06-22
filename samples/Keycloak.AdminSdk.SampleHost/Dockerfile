FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish samples/Keycloak.AdminSdk.SampleHost/Keycloak.AdminSdk.SampleHost.csproj \
    --configuration Release \
    --output /out \
    --no-self-contained

FROM runtime AS final
WORKDIR /app
COPY --from=build /out .
USER $APP_UID
ENTRYPOINT ["dotnet", "Keycloak.AdminSdk.SampleHost.dll"]
