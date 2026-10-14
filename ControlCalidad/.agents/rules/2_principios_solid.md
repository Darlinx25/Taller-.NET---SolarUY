# Principios SOLID y Buenas Prácticas de Código

El agente debe revisar activamente la lógica de los métodos generados o analizados para garantizar que cumplan con los principios SOLID, con especial énfasis en el Principio de Responsabilidad Única (SRP).

## 1. Principio de Responsabilidad Única (SRP) en Métodos
- **Una sola tarea:** Los métodos deben hacer exactamente lo que su nombre indica y nada más. No deben tener efectos secundarios ocultos.
- **Detección de Violaciones:** El agente debe marcar como incorrecto cualquier método que combine múltiples operaciones de negocio distintas.
  - *Ejemplo explícito de lo que NO se debe hacer:* Un método `RegistrarNodo(Nodo nodo, int cantidadPaneles)` que primero cree el `Inversor` asociado, luego dé de alta cada `Panel` del nodo y finalmente persista todo. Crear el inversor, crear los paneles y registrar el nodo son responsabilidades de negocio diferentes.
  - *Solución esperada:* Separar las responsabilidades. El caso de uso debe invocar primero a la lógica de creación del inversor (ej. `InversorService.CrearOCrear(...)`), luego a la de los paneles (ej. `PanelService.Instalar(...)`) y recién entonces pasar esos objetos al registro del nodo (ej. `NodoService.Registrar(...)`).

## 2. Complejidad Ciclomática y Tamaño
- **Métodos cortos:** Si un método requiere múltiples bloques `if/else` para manejar diferentes tareas no relacionadas directamente con su propósito principal, el agente debe sugerir extraer esa lógica a métodos privados o clases especializadas. El largo de los métodos debe ser limitado para que se puedan ver sin necesidad de hacer scroll (tomar un estimativo de referencia).

## 3. Nombrado que revele la intención
- Si el agente detecta que un método hace dos cosas justificadas, debe exigir que el nombre lo refleje (ej. `ObtenerOCrearInversor`). Si el nombre usa la conjunción "Y" o "And" (ej. `CrearParqueSolarYAsignarPaneles`), el agente debe advertir que probablemente se esté violando SRP y sugerir una refactorización.
