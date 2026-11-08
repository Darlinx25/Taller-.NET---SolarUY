// Arranque del mock en Node.js: carga el runtime .NET compilado a WebAssembly y ejecuta Program.cs.
import { dotnet } from './_framework/dotnet.js';

// El contenedor se detiene con SIGTERM: el mock no tiene estado que persistir, por lo que se sale de inmediato.
process.on('SIGTERM', () => process.exit(0));
process.on('SIGINT', () => process.exit(0));

// El runtime de .NET detecta el soporte de peticiones en streaming con `new Request("")`. En un navegador la URL
// vacía se resuelve contra la página, pero Node no tiene página y lanza "Failed to parse URL", lo que rompe
// todo POST con cuerpo. Definir el origen global de fetch (el mismo balanceador al que se envía) lo resuelve.
try {
    globalThis[Symbol.for('undici.globalOrigin.1')] = new URL(process.env.SimulacionApi__UrlBase ?? 'http://nginx:80');
} catch {
    // URL inválida: la validación de configuración de .NET informa el error con un mensaje claro.
}

// Las variables de entorno del proceso (SimulacionApi__*, Simulador__*) se pasan a .NET.
const { runMain } = await dotnet
    .withDiagnosticTracing(false)
    .withEnvironmentVariables(process.env)
    .create();

const codigoSalida = await runMain();
process.exit(codigoSalida);
