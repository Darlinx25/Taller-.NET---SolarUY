# Requerimientos: Gestión de Paneles

## Descripción General

Como Administrador del sistema SolarUY, quiero poder administrar los paneles fotovoltaicos asociados a un Nodo de un Parque Solar, para conocer qué paneles componen cada nodo, sus características y mantener actualizada esa información.

## Criterios de Aceptación

1. El Administrador debe poder acceder a la gestión de Paneles desde la ficha/detalle de un Nodo existente perteneciente a un Parque Solar.
2. El Administrador debe poder listar los Paneles registrados para un Nodo determinado.
3. El Administrador debe poder dar de alta un Panel fotovoltaico asociado a un Nodo, indicando sus características obligatorias.
4. El Administrador debe poder ver el detalle de un Panel, con sus datos y el Nodo/Parque Solar al que pertenece.
5. El Administrador debe poder editar los datos de un Panel existente.
6. El Administrador debe poder dar de baja (eliminar) un Panel, solicitando confirmación antes de la eliminación definitiva.
7. Solo los usuarios autenticados con el rol "Administrador" están autorizados a realizar estas operaciones.
8. El sistema debe informar el resultado de cada operación (éxito o error).

## Reglas de Negocio

- Un Panel pertenece obligatoriamente a un Nodo.
- Un Nodo pertenece obligatoriamente a un Parque Solar. Se debe mantener la jerarquía Parque Solar -> Nodo -> Panel.
- El Código/Serie del Panel es obligatorio, debe tener una longitud razonable y debe ser **único dentro del mismo Nodo**. No pueden existir dos Paneles con el mismo Código/Serie para el mismo Nodo.
- Al dar de alta se deben registrar, como mínimo: Código/Serie, Marca, Modelo, Potencia (Watt pico - Wp).
- La Potencia (Wp) debe ser un valor numérico mayor a 0 cuando se informa.
- La fecha de alta/creación del Panel se registra al momento del alta y **no se modifica** al editar.
- La baja del Panel es **física** (eliminación definitiva). Se debe solicitar confirmación al Administrador antes de ejecutar la eliminación.
- Solo se pueden crear/editar/eliminar Paneles sobre Nodos existentes.
- Las validaciones deben aplicarse tanto a nivel de UI como a nivel de Dominio.
- Únicamente el rol "Administrador" puede acceder a estas operaciones.
