# Requerimientos: Gestión de Nodos e Inversores

## Descripción General

Como Administrador del sistema SolarUY, quiero poder configurar los Nodos de un Parque Solar y su Inversor asociado, para administrar la topología del parque antes de registrar paneles (funcionalidad futura).

## Criterios de Aceptación

1. El Administrador debe poder acceder a la gestión de Nodos e Inversores desde la ficha/detalle de un Parque Solar existente.
2. El Administrador debe poder listar los Nodos registrados para un Parque Solar determinado.
3. El Administrador debe poder crear un Nodo asociado a un Parque Solar, incluyendo los datos del Inversor que se le asignará.
4. El Administrador debe poder ver el detalle de un Nodo, incluyendo la información de su Inversor asociado.
5. El Administrador debe poder editar los datos del Nodo y/o los datos de su Inversor asociado.
6. El Administrador debe poder dar de baja (eliminar) un Nodo, solicitando confirmación antes de la eliminación definitiva.
7. Solo los usuarios autenticados con el rol "Administrador" están autorizados a realizar estas operaciones.
8. El sistema debe informar el resultado de cada operación (éxito o error).

## Reglas de Negocio

- Un Nodo pertenece obligatoriamente a un Parque Solar.
- Cada Nodo debe tener exactamente un Inversor asociado (relación 1:1 entre Nodo e Inversor).
- El código del Nodo es obligatorio, debe tener una longitud máxima razonable y debe ser único **dentro del mismo Parque Solar**. No pueden existir dos Nodos con el mismo código para el mismo parque.
- Los datos del Inversor (al menos su identificación/marca/modelo o código/serie según corresponda) son obligatorios al momento de crear el Nodo, ya que ambos se configuran juntos en este requerimiento.
- No se puede crear/editar un Nodo con datos inválidos. Las validaciones deben aplicarse también a nivel de Dominio.
- La fecha de creación del Nodo (y su Inversor asociado) se registra al momento del alta y no se modifica al editar.
- La baja de un Nodo es **física** (eliminación definitiva). Se debe solicitar confirmación al Administrador antes de ejecutar la eliminación.
- Las operaciones se realizan exclusivamente sobre Parques Solares existentes.
- Las responsabilidades de creación/configuración de Nodo e Inversor se tratan como partes del mismo caso de uso de configuración de topología, respetando la separación de responsabilidades entre entidades.
