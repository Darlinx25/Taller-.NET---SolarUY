# Requerimientos: Mejoras visuales y de experiencia de usuario

## Descripcion General

Como usuario de SolarUY (Administrador u Operador), quiero una interfaz mas
clara, consistente y practica para gestionar parques solares, nodos, paneles y
consultar el estado del sistema, para realizar las tareas cotidianas con menos
pasos y sin perderme en pantallas intermedias.

El foco principal es la gestion jerarquica Parque Solar -> Nodo -> Panel. Hoy,
para agregar un nodo hay que entrar al detalle del parque y, para agregar un
panel, hay que entrar al detalle del nodo. Se quiere poder hacerlo desde las
mismas pantallas de gestion y volver al contexto de origen al terminar.

Ademas, se busca que todo el sitio tenga una apariencia uniforme y profesional
utilizando unicamente los recursos que ya existen en el proyecto (Bootstrap,
HTML, CSS y JavaScript), sin incorporar librerias externas nuevas.

## Criterios de Aceptacion

1. El layout general del sitio presenta una navegacion clara, con el modulo
   activo resaltado y accesos coherentes segun el rol del usuario.
2. Todas las pantallas muestran migas de pan (breadcrumbs) que permiten ubicarse
   en la jerarquia Parque Solar -> Nodo -> Panel -> Tareas y volver a un nivel
   superior.
3. Los mensajes de exito y error se muestran de forma consistente y visible en
   cualquier pantalla, sin duplicarse.
4. El listado de parques solares permite iniciar el alta de un nodo sin tener
   que entrar primero al detalle del parque.
5. El listado de nodos permite iniciar el alta de un panel sin tener que entrar
   primero al detalle del nodo.
6. El detalle del parque solar muestra, en la misma pantalla, sus nodos y los
   paneles de cada nodo, con acciones para agregar, editar, eliminar y ver el
   detalle de cada elemento.
7. El detalle del nodo muestra, en la misma pantalla, sus paneles y las acciones
   para agregarlos, editarlos, eliminarlos y gestionar sus tareas de
   mantenimiento.
8. Los formularios de alta, edicion y baja de parques, nodos y paneles se
   presentan de forma ordenada, agrupando los datos relacionados y con acciones
   claras de guardar y cancelar.
9. Al crear o editar un nodo o panel desde un contexto determinado (por ejemplo,
   el detalle de un parque o de un nodo), el usuario regresa a ese contexto al
   finalizar la operacion.
10. Los listados y detalles muestran estados vacios claros con una accion
    sugerida cuando no hay datos.
11. Los estados de nodos y paneles se muestran con indicadores visuales
    (badges/etiquetas) que permiten distinguirlos de un vistazo.
12. Las pantallas de Dashboard, Alertas, Historico, Tareas de mantenimiento,
    Inicio, Error y Acceso denegado mantienen su funcionalidad actual pero
    adoptan el mismo estilo visual y de navegacion.
13. Las tablas son legibles y utilizables en pantallas pequenas, sin perder
    informacion ni acciones.
14. No se incorporan librerias externas nuevas; se utilizan Bootstrap, HTML,
    CSS y JavaScript ya disponibles en el proyecto.
15. Las rutas, acciones y permisos existentes se mantienen sin cambios
    funcionales.

## Reglas de Negocio

- La jerarquia funcional es Parque Solar -> Nodo -> Panel -> Tareas de
  mantenimiento. La interfaz debe respetarla y hacerla visible.
- Un nodo pertenece a un unico parque solar y un panel pertenece a un unico
  nodo.
- Las acciones de alta, edicion y baja de parques, nodos y paneles siguen
  reservadas al rol Administrador.
- El Dashboard, las Alertas y el Historico siguen disponibles para los roles que
  ya tienen acceso en la actualidad.
- La visualizacion no altera las reglas de negocio ni las validaciones
  existentes; solo cambia la forma de presentar y navegar.
- Los mensajes al usuario deben ser claros, en espanol y sin exponer detalles
  tecnicos.
- No se debe modificar la documentacion README del proyecto en este
  requerimiento.

## Fuera de Alcance

- Cambios en el modelo de dominio, la base de datos o las migraciones.
- Cambios en las reglas de negocio, validaciones o permisos existentes.
- Nuevas funcionalidades de negocio (reportes nuevos, filtros nuevos,
  integraciones nuevas).
- Incorporacion de librerias, frameworks de UI o paquetes de iconos externos.
- Cambios en la autenticacion OIDC, el servidor MCP, la simulacion o la
  telemetria.
- Rediseno de la identidad visual de la marca (logo, paleta corporativa nueva).
- Modificacion de los archivos README del repositorio.
