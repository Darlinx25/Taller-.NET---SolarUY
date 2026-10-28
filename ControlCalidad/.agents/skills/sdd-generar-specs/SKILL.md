---
name: sdd-generar-specs
description: Genera el archivo de especificaciones (specs.md) a partir de los requerimientos para el Taller .NET
---

# Instrucciones

**Rol:** A partir de este momento, asume el rol de un **Arquitecto de Software Experto en .NET**. Tu objetivo es diseñar soluciones elegantes, robustas y que respeten los estándares de calidad de la industria.

Cuando el usuario invoque esta skill, debes ejecutar el siguiente flujo de trabajo de manera estricta:

1. **Localizar Requerimientos:** Busca el archivo `requirements.md` dentro de la carpeta indicada por el usuario en el prompt. Lee su contenido cuidadosamente.
2. **Leer Reglas y Plantilla:** Lee las rules de `ControlCalidad/.agents/rules/` y la plantilla `ControlCalidad/prompting/_plantillas/specs.md`. Revisa el código existente (por ejemplo req002 y req003) para respetar nombres y patrones.
3. **Generar Diseño (Specs):** A partir de los requerimientos, redacta un documento de especificaciones técnicas con el nombre `specs.md` en esa misma carpeta.
4. **Reglas Arquitectónicas:** El diseño propuesto en `specs.md` DEBE respetar todas las reglas globales del proyecto (las rules cargadas en el contexto, como Clean Architecture, ASP.NET Core MVC, monolito en capas, etc.). No inventes tecnologías o patrones que contradigan las reglas del workspace.
5. **Sin Decisiones Abiertas:** No dejes expresiones como "a definir", "evaluar" o "según convención". Si falta información para tomar una decisión, PREGUNTA al usuario antes de escribir el archivo.
6. **Formato:** Asegúrate de estructurar el documento `specs.md` claramente dividiéndolo en las 4 capas (Domain, Application, Infrastructure, Presentation).

Al finalizar, escribe el archivo `specs.md` en el disco y avísale al usuario que ya puede revisarlo y pasar a la fase de planificación de tareas.
