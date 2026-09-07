# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Company.ProjectName.slnx --locked-mode
RUN dotnet publish src/Company.ProjectName.Api/Company.ProjectName.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_EnableDiagnostics=0

COPY --from=build --chown=app:app /app/publish .
USER app

ENTRYPOINT ["dotnet", "Company.ProjectName.Api.dll"]
