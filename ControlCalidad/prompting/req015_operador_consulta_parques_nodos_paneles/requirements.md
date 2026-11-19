# Requerimientos: consulta de parques, nodos y paneles para Operador

## Descripcion General

Como Operador, quiero consultar los parques solares, sus nodos y sus paneles
para conocer el estado de las instalaciones sin poder modificar la
configuracion del sistema.

## Criterios de Aceptacion

1. El Operador puede ingresar a la seccion de parques solares desde el inicio y
   desde el menu principal.
2. El Operador puede ver el listado de parques solares y el detalle de cada
   parque, incluidos sus nodos y paneles.
3. El Operador puede ver el listado de nodos de un parque y el detalle de cada
   nodo.
4. El Operador puede ver el listado de paneles de un nodo y el detalle de cada
   panel.
5. El Operador no visualiza acciones para crear, editar, eliminar ni administrar
   tareas de mantenimiento.
6. Si el Operador intenta acceder directamente a una accion de creacion,
   edicion o eliminacion, el sistema le deniega el acceso.
7. El Administrador conserva todas las acciones de gestion que tenia antes.
8. Las pantallas de consulta del Operador mantienen el mismo estilo y la misma
   navegacion que las del Administrador.

## Reglas de Negocio

- El Operador tiene acceso de solo lectura a parques solares, nodos y paneles.
- Las tareas de mantenimiento siguen siendo exclusivas del Administrador.
- La falta de botones en pantalla no es suficiente: toda accion de gestion debe
  estar protegida del lado del servidor.
- El Operador debe poder navegar por las relaciones parque, nodo y panel sin
  perder el contexto.

## Fuera de Alcance

- Crear, editar o eliminar parques solares, nodos o paneles como Operador.
- Consultar tareas de mantenimiento como Operador.
- Cambiar el comportamiento del Administrador.
- Crear una seccion o ruta separada exclusiva para el Operador.
- Modificar reglas de negocio del Dominio, la base de datos o las migraciones.
