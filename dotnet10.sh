#!/usr/bin/env bash
# Containerized .NET 10 CLI for this repo. Usage: ./dotnet10.sh <args>
set -e
docker run --rm \
  --user "$(id -u):$(id -g)" \
  -e DOTNET_CLI_HOME=/tmp/dotnet \
  -e DOTNET_NOLOGO=1 \
  -e NUGET_PACKAGES=/home/nuget \
  -v "$PWD:/src" \
  -v "$HOME/.nuget-packages-dotnet10:/home/nuget" \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet "$@"
