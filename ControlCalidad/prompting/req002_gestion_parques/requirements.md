# Requerimientos: Gestión de Parques Solares

## Descripción General
Como Administrador del sistema SolarUY, quiero consultar y administrar los parques solares ya registrados para mantener actualizada su información y poder dar de baja los que ya no correspondan.

## Criterios de Aceptación
1. El Administrador debe poder ver el detalle de un parque solar.
2. El Administrador debe poder modificar el nombre, la ubicación y la capacidad estimada de un parque existente.
3. El Administrador debe poder eliminar un parque solar, solicitando confirmación antes de la baja.
4. Solo los usuarios autenticados con rol "Administrador" pueden realizar estas operaciones.
5. El sistema debe informar el resultado de cada operación (éxito o error).

## Reglas de Negocio
- El nombre del parque sigue siendo obligatorio y único en el sistema; no puede repetirse al editar.
- La ubicación es obligatoria y la capacidad estimada no puede ser negativa.
- La fecha de creación no se modifica al editar el parque.
- La eliminación de un parque es definitiva y deja de mostrarse en el listado.
