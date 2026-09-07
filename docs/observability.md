# Observability

The API uses vendor-neutral OpenTelemetry instrumentation.

## Included

- Structured logs through the built-in `ILogger<T>` abstraction.
- ASP.NET Core request traces and metrics.
- `HttpClient` traces and metrics.
- .NET runtime metrics.
- `/health/live` for process liveness.
- `/health/ready` for PostgreSQL readiness.
- OpenAPI JSON and Scalar in Development.
- Optional OTLP export controlled by `OpenTelemetry:OtlpEndpoint`.

Without an OTLP endpoint configured, the API remains functional and does not require an observability service.

## Local collector

```bash
docker compose \
  -f compose.yaml \
  -f compose.observability.yaml \
  --profile observability up -d otel-collector
```

Set the endpoint for a locally running API:

```text
OpenTelemetry__OtlpEndpoint=http://localhost:4317
```

The collector configuration exports received traces and metrics to its debug exporter. Replace that exporter in a deployment-specific configuration when connecting Grafana, Jaeger, Application Insights or another backend.
