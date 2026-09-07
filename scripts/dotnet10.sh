#!/usr/bin/env bash
# Run the .NET 10 CLI without installing the SDK on the host.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RYUK_DISABLED="${TESTCONTAINERS_RYUK_DISABLED:-false}"

exec docker run --rm \
  --network host \
  --user "$(id -u):$(id -g)" \
  --group-add "$(stat -c '%g' /var/run/docker.sock)" \
  -e DOTNET_CLI_HOME=/tmp/dotnet \
  -e DOTNET_NOLOGO=1 \
  -e TESTCONTAINERS_RYUK_DISABLED="$RYUK_DISABLED" \
  -e NUGET_PACKAGES=/home/nuget \
  -v "$ROOT:/src" \
  -v "$HOME/.nuget-packages-dotnet10:/home/nuget" \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -w /src \
  mcr.microsoft.com/dotnet/sdk:10.0 \
  dotnet "$@"
