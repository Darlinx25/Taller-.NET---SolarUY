// Indicador de alertas activas del menú: consulta la cantidad al cargar la página y se actualiza en vivo
// con el evento "ActualizarAlertas" del hub de SignalR, sin recargar.
(function () {
    const contador = document.getElementById('alertasContador');
    if (!contador) {
        return;
    }

    const urlCantidad = contador.dataset.url;

    async function actualizar() {
        try {
            const respuesta = await fetch(urlCantidad, { credentials: 'same-origin' });
            if (!respuesta.ok) {
                return;
            }

            const datos = await respuesta.json();
            const cantidad = Number(datos.cantidadActivas) || 0;
            contador.textContent = cantidad;
            contador.classList.toggle('d-none', cantidad === 0);
        } catch (error) {
            console.error(error.toString());
        }
    }

    actualizar();

    if (typeof signalR === 'undefined') {
        return;
    }

    // Con varias réplicas detrás del balanceador (sin sesión pegajosa) la negociación y el WebSocket pueden caer
    // en réplicas distintas y la conexión falla. Conectar directo por WebSocket evita la negociación; los eventos
    // entre réplicas viajan por el backplane de Redis.
    const conexion = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/telemetria', {
            skipNegotiation: true,
            transport: signalR.HttpTransportType.WebSockets
        })
        .withAutomaticReconnect()
        .build();

    conexion.on('ActualizarAlertas', actualizar);
    conexion.onreconnected(actualizar);

    conexion.start().catch(function (error) {
        console.error(error.toString());
    });
})();
