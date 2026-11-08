using Microsoft.Extensions.Logging;

namespace SolarUY.Simulador.Wasm;

/// <summary>
/// Proveedor de logs mínimo que escribe una línea por mensaje en la salida estándar. El logger de consola
/// de Microsoft.Extensions.Logging no está soportado en WebAssembly.
/// </summary>
public sealed class ProveedorLoggerConsola : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new LoggerConsola(categoryName);

    public void Dispose()
    {
    }

    private sealed class LoggerConsola(string categoria) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var linea = $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ} {Nivel(logLevel)} {categoria}: {formatter(state, exception)}";
            if (exception is not null)
            {
                linea += $" | {exception.GetType().Name}: {exception.Message}";
            }

            Console.WriteLine(linea);
        }

        private static string Nivel(LogLevel nivel) => nivel switch
        {
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => nivel.ToString().ToLowerInvariant(),
        };
    }
}
