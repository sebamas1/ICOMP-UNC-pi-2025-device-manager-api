namespace DeviceAPI.Manager.Web.Middleware;

/// <summary>
/// Middleware de fault injection para pruebas de canary deployments y rollback automático (Lab 5).
/// Cuando "FaultInjection:ErrorRate" (0.0 a 1.0) es mayor a 0, responde 500 para esa proporción
/// de requests, simulando una versión "con bug" cuyo error rate debe detectar Prometheus.
/// Deshabilitado por defecto (ErrorRate = 0) — no afecta el comportamiento normal de la API.
/// </summary>
public class FaultInjectionMiddleware(RequestDelegate next, IConfiguration configuration)
{
    // Rutas que nunca deben fallar a propósito: liveness/readiness, métricas y documentación,
    // para no confundir el rollback automático con reinicios de pod por health checks.
    private static readonly string[] ExcludedPathPrefixes =
        ["/health", "/api/devices/health", "/metrics", "/swagger", "/openapi"];

    public async Task InvokeAsync(HttpContext context)
    {
        var errorRate = configuration.GetValue<double>("FaultInjection:ErrorRate", 0);

        var shouldInject = errorRate > 0
            && !ExcludedPathPrefixes.Any(p => context.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase))
            && Random.Shared.NextDouble() < errorRate;

        if (shouldInject)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { error = "Fault injected (testing de canary/rollback)" });
            return;
        }

        await next(context);
    }
}
