# Simulador (mock de la placa ARM)

Mock del inversor y los sensores ARM de SolarUY. Es un modulo **separado de la aplicacion**: no comparte
codigo con ella, no accede a MySQL y solo habla con el balanceador de carga (`nginx`) por HTTP.

La logica es .NET compilado a **WebAssembly** (`browser-wasm`) y se ejecuta con **Node.js** dentro de su propio
contenedor. No usa navegador ni interfaz web: se levanta junto con el resto del stack.

## Que hace

Cada 5 segundos:

1. Pide la topologia registrada: `GET /api/simulacion/topologia`.
2. Genera la lectura de cada parque, nodo `Activo` y panel (curva solar de 07:00 a 19:00 hora de Uruguay,
   variacion aleatoria, fallas con recuperacion automatica y energia acumulada).
3. La envia: `POST /api/simulacion/telemetria`, con reintentos.

Ambos endpoints se autentican con el encabezado `X-Api-Key`. Si la topologia no se puede obtener, se omite el tick
y la aplicacion conserva la ultima lectura.

## Control desde el dashboard

En cada ciclo el mock tambien consulta la orden vigente: `GET /api/simulacion/control` (con `X-Api-Key`). Es el unico
camino de la aplicacion al mock, que no abre puertos. El administrador la cambia desde los botones del dashboard:

- **Forzar dia / Volver a hora real**: la simulacion ya corre sola con el stack; este boton solo hace que, mientras este forzado, el mock genere como si fueran las 13:00 (pico)
  sin importar la hora real (las fallas aleatorias siguen funcionando); al volver a hora real genera segun el reloj.
- **Simular falla**: todos los paneles de los nodos activos pasan a falla (0 W, `EnFalla`) y se recuperan
  solos con la misma probabilidad de recuperacion por lectura que una falla aleatoria.

El efecto se ve en la lectura siguiente (hasta 5 segundos) y el dashboard se actualiza solo por SignalR. Si no se puede
consultar la orden, el mock conserva la ultima conocida. La orden se pierde al reiniciar la aplicacion.

## Estructura

```text
Simulador/
├── Dockerfile                         # build (SDK + wasm-tools) y ejecucion (Node.js 22)
├── .dockerignore
├── src/
│   ├── SolarUY.Simulador/             # biblioteca: Domain, Contracts, Simulation, Transport
│   └── SolarUY.Simulador.Wasm/        # arranque WebAssembly: Program.cs, main.mjs, LoggerConsola.cs
└── tests/
    └── SolarUY.Simulador.Tests/       # tests xUnit de la logica pura
```

`SolarUY.Simulador.Wasm` no esta en `SolarUY.sln`: compilarlo exige la carga de trabajo `wasm-tools`, que la imagen
`tools` no tiene. Se construye unicamente con el `Dockerfile` de esta carpeta. La biblioteca y los tests si estan en
la solucion.

## Configuracion (variables de entorno)

| Variable | Defecto | Descripcion |
|---|---|---|
| `SimulacionApi__UrlBase` | `http://nginx:80` | URL del balanceador |
| `SimulacionApi__ApiKey` | (obligatoria) | Misma clave que la `app` (`SIMULACION_API_KEY`) |
| `SimulacionApi__TimeoutSegundos` | `10` | Timeout de cada pedido HTTP |
| `Simulador__IntervaloSegundos` | `5` | Intervalo entre lecturas |
| `Simulador__ProbabilidadFalla` | `0.20` | Probabilidad de que un panel operativo falle en cada lectura (`SIMULADOR_PROBABILIDAD_FALLA` en `.env`) |
| `Simulador__ProbabilidadRecuperacion` | `0.40` | Probabilidad de que un panel en falla se recupere en cada lectura; una falla dura en promedio `1 / probabilidad` lecturas, unos 12 s (`SIMULADOR_PROBABILIDAD_RECUPERACION` en `.env`) |
| `Simulador__VariacionMaxima` | `0.10` | Variacion aleatoria maxima de la potencia |
| `Simulador__DesfaseHorarioUruguayHoras` | `-3` | Desfase horario respecto de UTC |
| `Simulador__IntentosEnvio` | `3` | Intentos de envio por lectura |
| `Simulador__EsperaEntreIntentosSegundos` | `2` | Espera entre intentos |

Con configuracion invalida el modulo informa cada error y termina con codigo de salida distinto de 0.

## Uso

```bash
docker compose up -d --build           # levanta el stack completo, incluido el simulador
docker compose logs -f simulador       # una lectura generada y un envio cada 5 s
docker compose up -d --build --no-deps simulador   # reconstruir solo el simulador
docker compose stop simulador          # cierre inmediato
```

## Pruebas

```bash
docker compose run --rm tools dotnet test Simulador/tests/SolarUY.Simulador.Tests
```

## Notas tecnicas

- `main.mjs` define el origen global de `fetch` de Node: el runtime de .NET detecta el soporte de peticiones en
  streaming con `new Request("")`, que en Node (sin pagina) falla y rompe todo `POST` con cuerpo.
- El logger de consola estandar de .NET no esta soportado en WebAssembly; `LoggerConsola.cs` escribe cada mensaje
  con `Console.WriteLine`.
- `WasmBuildNative=false` usa el runtime ya compilado: no hace falta Emscripten ni Python.
- El recorte de codigo esta desactivado porque la serializacion JSON de los DTOs usa reflexion.
