# Requerimientos: Estado tipado de Nodos

## Descripcion General

Como Administrador del sistema SolarUY, quiero seleccionar el estado de un Nodo
desde una lista cerrada de opciones para no tener que adivinar ni escribir
valores libres al crear o editar un nodo.

## Criterios de Aceptacion

1. Los formularios de alta y edicion de Nodo muestran un combo con los estados
   permitidos en lugar de un campo de texto libre.
2. El combo muestra etiquetas legibles: `Activo`, `Inactivo` y
   `En mantenimiento` (con espacio, no `EnMantenimiento`).
3. Los badges de estado de Nodo muestran la misma etiqueta legible.
4. Un estado fuera de la lista es rechazado por la validacion de entrada y por
   el Dominio.
5. Los valores existentes en la base de datos que sean equivalentes a un estado
   valido se normalizan antes de aplicar la restriccion de base de datos.
6. El contrato de topologia que consume el simulador y el servidor MCP sigue
   entregando el estado como texto, sin romper su funcionamiento.
7. Los tests del Dominio, los guardarrailes de ArchUnitNET y el build quedan en
   verde.

## Reglas de Negocio

- Los estados validos de un Nodo son `Activo`, `Inactivo` y
  `EnMantenimiento`.
- Solo los nodos `Activo` participan de la simulacion de telemetria.
- `EnFalla` no es un estado administrativo de Nodo: es un estado de salud de
  panel/telemetria.
- El estado por defecto al crear un Nodo es `Activo`.
- La base de datos solo acepta los tres estados definidos para Nodo.

## Fuera de Alcance

- Cambiar los estados de paneles, alertas o tareas de mantenimiento.
- Cambiar el contrato JSON de `GET /api/simulacion/topologia`.
- Agregar nuevos estados de Nodo sin una definicion de negocio explicita.
