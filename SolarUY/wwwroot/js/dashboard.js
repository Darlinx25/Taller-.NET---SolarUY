(function () {
    const root = document.getElementById('dashboardRoot');
    if (!root) {
        return;
    }

    const contenidoUrl = root.dataset.contenidoUrl;
    const tendenciaUrl = root.dataset.tendenciaUrl;
    const urlSimulacionForzada = root.dataset.simulacionUrl;
    const urlFalla = root.dataset.fallaUrl;
    const mensaje = document.getElementById('controlMensaje');
    const colores = {
        primario: '#0d6efd',
        exito: '#198754',
        peligro: '#dc3545',
        gris: '#6c757d'
    };
    const graficosEnVivo = {
        parque: null,
        estados: null,
        nodo: null
    };
    let graficoTendencia = null;
    let actualizacionEnCurso = false;
    let actualizacionPendiente = false;

    function parsearDatos(id) {
        const elemento = document.getElementById(id);
        if (!elemento) {
            return null;
        }

        try {
            return JSON.parse(elemento.textContent || 'null');
        } catch (error) {
            console.error('No se pudieron leer los datos de la gráfica.', error);
            return null;
        }
    }

    function formatearNumero(valor) {
        return Number(valor || 0).toLocaleString('es-UY', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function formatearPeriodo(periodo) {
        const partes = String(periodo || '').split('-');
        if (partes.length !== 3) {
            return periodo;
        }

        return partes[2] + '/' + partes[1];
    }

    function destruirGraficosEnVivo() {
        Object.keys(graficosEnVivo).forEach(function (clave) {
            if (graficosEnVivo[clave]) {
                graficosEnVivo[clave].destroy();
                graficosEnVivo[clave] = null;
            }
        });
    }

    function inicializarGraficoParque() {
        const canvas = document.getElementById('grafico-generacion-parque');
        const datos = parsearDatos('datos-generacion-parque');

        if (!canvas || !datos || typeof Chart === 'undefined') {
            return;
        }

        graficosEnVivo.parque = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: datos.map(function (dato) {
                    return dato.Etiqueta;
                }),
                datasets: [{
                    label: 'Generación (W)',
                    data: datos.map(function (dato) {
                        return dato.Valor;
                    }),
                    backgroundColor: colores.primario,
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'W'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function (contexto) {
                                return formatearNumero(contexto.raw) + ' W';
                            }
                        }
                    }
                }
            }
        });
    }

    function inicializarGraficoEstados() {
        const canvas = document.getElementById('grafico-estados-paneles');
        const datos = parsearDatos('datos-estados-paneles');

        if (!canvas || !datos || typeof Chart === 'undefined') {
            return;
        }

        graficosEnVivo.estados = new Chart(canvas, {
            type: 'doughnut',
            data: {
                labels: ['Operativo', 'EnFalla', 'Sin telemetría'],
                datasets: [{
                    data: [datos.Operativos, datos.EnFalla, datos.SinTelemetria],
                    backgroundColor: [colores.exito, colores.peligro, colores.gris],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        callbacks: {
                            label: function (contexto) {
                                return contexto.label + ': ' + contexto.raw;
                            }
                        }
                    }
                }
            }
        });
    }

    function inicializarGraficoNodo() {
        const canvas = document.getElementById('grafico-generacion-nodo');
        const datos = parsearDatos('datos-generacion-nodo');

        if (!canvas || !datos || typeof Chart === 'undefined') {
            return;
        }

        graficosEnVivo.nodo = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: datos.map(function (dato) {
                    return dato.Etiqueta;
                }),
                datasets: [{
                    label: 'Generación (W)',
                    data: datos.map(function (dato) {
                        return dato.Valor;
                    }),
                    backgroundColor: colores.exito,
                    borderRadius: 4
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'W'
                        }
                    },
                    y: {
                        ticks: {
                            autoSkip: false
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function (contexto) {
                                return formatearNumero(contexto.raw) + ' W';
                            }
                        }
                    }
                }
            }
        });
    }

    function inicializarGraficosEnVivo() {
        destruirGraficosEnVivo();
        inicializarGraficoParque();
        inicializarGraficoEstados();
        inicializarGraficoNodo();
    }

    function inicializarGraficoTendencia() {
        const canvas = document.getElementById('grafico-tendencia');
        const datos = parsearDatos('datos-tendencia') || [];

        if (!canvas || typeof Chart === 'undefined') {
            return;
        }

        graficoTendencia = new Chart(canvas, {
            type: 'line',
            data: {
                labels: datos.map(function (dato) {
                    return formatearPeriodo(dato.Periodo);
                }),
                datasets: [{
                    label: 'Energía (kWh)',
                    data: datos.map(function (dato) {
                        return dato.EnergiaKWh;
                    }),
                    borderColor: colores.primario,
                    backgroundColor: 'rgba(13, 110, 253, 0.15)',
                    fill: true,
                    tension: 0.25,
                    pointRadius: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'kWh'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            label: function (contexto) {
                                return formatearNumero(contexto.raw) + ' kWh';
                            }
                        }
                    }
                }
            }
        });
    }

    async function actualizarTendencia() {
        if (!tendenciaUrl) {
            return;
        }

        try {
            const response = await fetch(tendenciaUrl, { credentials: 'same-origin' });
            if (!response.ok) {
                throw new Error('No se pudo actualizar la tendencia (' + response.status + ').');
            }

            const tendencia = await response.json();
            const puntos = tendencia.Puntos || [];
            const tieneDatos = Boolean(tendencia.TieneDatos) && puntos.length > 0;
            const contenedor = document.getElementById('contenedorGraficoTendencia');
            const mensajeTendencia = document.getElementById('mensajeTendencia');

            if (contenedor) {
                contenedor.classList.toggle('d-none', !tieneDatos);
            }

            if (mensajeTendencia) {
                mensajeTendencia.classList.toggle('d-none', tieneDatos);
            }

            if (graficoTendencia) {
                graficoTendencia.data.labels = puntos.map(function (dato) {
                    return formatearPeriodo(dato.Periodo);
                });
                graficoTendencia.data.datasets[0].data = puntos.map(function (dato) {
                    return dato.EnergiaKWh;
                });
                graficoTendencia.update();
                graficoTendencia.resize();
            } else if (tieneDatos) {
                inicializarGraficoTendencia();
            }
        } catch (error) {
            console.error(error.toString());
        }
    }

    async function actualizarContenido() {
        if (actualizacionEnCurso) {
            actualizacionPendiente = true;
            return;
        }

        actualizacionEnCurso = true;

        try {
            const response = await fetch(contenidoUrl, { credentials: 'same-origin' });
            if (!response.ok) {
                throw new Error('No se pudo actualizar el dashboard (' + response.status + ').');
            }

            const html = await response.text();
            const contenedor = document.getElementById('dashboardLive');

            destruirGraficosEnVivo();

            if (contenedor) {
                contenedor.innerHTML = html;
                inicializarGraficosEnVivo();
            }
        } catch (error) {
            console.error(error.toString());
        } finally {
            actualizacionEnCurso = false;

            if (actualizacionPendiente) {
                actualizacionPendiente = false;
                actualizarContenido();
            }
        }
    }

    function mostrarMensaje(texto, clase) {
        if (!mensaje) {
            return;
        }

        mensaje.className = 'alert ' + clase;
        mensaje.textContent = texto;
    }

    root.addEventListener('click', async function (evento) {
        const objetivo = evento.target;
        const boton = objetivo instanceof Element
            ? objetivo.closest('button[data-accion]')
            : null;

        if (!boton) {
            return;
        }

        const esFalla = boton.dataset.accion === 'falla';
        const activa = boton.dataset.activa === 'true';
        boton.disabled = true;

        try {
            const response = await fetch(esFalla ? urlFalla : urlSimulacionForzada, {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(esFalla ? {} : { activa: activa })
            });

            if (response.status === 401 || response.status === 403) {
                mostrarMensaje('No tiene permiso para controlar la simulación.', 'alert-danger');
                return;
            }

            if (!response.ok) {
                throw new Error('Respuesta ' + response.status);
            }

            mostrarMensaje(
                esFalla
                    ? 'Falla solicitada: los paneles fallarán en la próxima lectura (hasta 5 segundos).'
                    : (activa
                        ? 'Día forzado: se generará como a pleno sol desde la próxima lectura (hasta 5 segundos).'
                        : 'Hora real: se volverá a generar según la hora real desde la próxima lectura (hasta 5 segundos).'),
                'alert-success');
        } catch (error) {
            console.error(error.toString());
            mostrarMensaje('No se pudo enviar la orden de simulación.', 'alert-danger');
        } finally {
            boton.disabled = false;
        }
    });

    if (typeof signalR !== 'undefined') {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/telemetria', {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .build();

        connection.on('ActualizarTelemetria', function () {
            actualizarContenido();
        });

        connection.start().catch(function (error) {
            console.error(error.toString());
        });
    }

    inicializarGraficosEnVivo();
    inicializarGraficoTendencia();
    window.setInterval(actualizarTendencia, 5 * 60 * 1000);
})();
