(function () {
    function cerrarAlertasAutomaticas() {
        if (typeof bootstrap === 'undefined') {
            return;
        }

        document.querySelectorAll('[data-auto-dismiss="true"]').forEach(function (elemento) {
            window.setTimeout(function () {
                bootstrap.Alert.getOrCreateInstance(elemento).close();
            }, 5000);
        });
    }

    function enfocarPrimerError() {
        const primerError = document.querySelector('.input-validation-error');
        if (primerError && typeof primerError.focus === 'function') {
            primerError.focus();
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        cerrarAlertasAutomaticas();
        enfocarPrimerError();
    });
})();
