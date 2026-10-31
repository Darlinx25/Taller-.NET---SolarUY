(function () {
    const tipoSelect = document.getElementById('Filtro_TipoEntidad');
    const entidadSelect = document.getElementById('Filtro_EntidadId');

    function filtrarEntidades() {
        if (!tipoSelect || !entidadSelect) {
            return;
        }

        const tipo = tipoSelect.value;

        Array.from(entidadSelect.options).forEach(function (option) {
            const tipoOpcion = option.dataset.tipo;
            const visible = !tipoOpcion || tipoOpcion === tipo;
            option.hidden = !visible;
            option.disabled = !visible;
        });

        const seleccionada = entidadSelect.selectedOptions[0];
        if (seleccionada && seleccionada.disabled) {
            entidadSelect.value = '';
        }
    }

    if (tipoSelect) {
        tipoSelect.addEventListener('change', filtrarEntidades);
        filtrarEntidades();
    }

    const datos = document.getElementById('datos-grafico');
    const canvas = document.getElementById('grafico-historico');

    if (!datos || !canvas || typeof Chart === 'undefined') {
        return;
    }

    const puntos = JSON.parse(datos.textContent || '[]');
    const etiquetas = puntos.map(function (punto) {
        return punto.Periodo;
    });
    const energia = puntos.map(function (punto) {
        return punto.EnergiaKWh;
    });

    new Chart(canvas, {
        type: 'line',
        data: {
            labels: etiquetas,
            datasets: [{
                label: 'Energía (kWh)',
                data: energia,
                borderColor: '#0d6efd',
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
                },
                x: {
                    title: {
                        display: true,
                        text: 'Período'
                    }
                }
            },
            plugins: {
                legend: {
                    display: true
                }
            }
        }
    });
})();
