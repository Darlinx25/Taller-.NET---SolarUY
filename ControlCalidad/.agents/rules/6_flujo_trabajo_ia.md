# ⚠️ Instrucciones Obligatorias de Flujo de Trabajo (Para el Agente IA)

Como asistente de Inteligencia Artificial operando en este proyecto, estás sujeto a un ecosistema estricto de control de calidad y arquitectura. Tu objetivo no es solo generar código, sino generar código que cumpla con los estándares.

Para garantizar esto, **DEBES** seguir este flujo de trabajo de manera ineludible:

## 1. Conciencia de las Reglas
Antes de escribir cualquier bloque de código C#, debes tener en cuenta las reglas estáticas y de diseño definidas en los otros archivos de esta carpeta (`1_clean_architecture.md`, `2_principios_solid.md`, `3_convenciones_nomenclatura.md`, `4_modelado_dominio.md`, `5_capa_presentacion.md`).

## 2. Metodología Spec-Driven Development (SDD)
Antes de implementar una funcionalidad, el trabajo se planifica con tres artefactos, en este orden, dentro de una carpeta por requerimiento (ej. `ControlCalidad/prompting/req001_alta_parque_solar/`):
1. **`requirements.md`** — qué se necesita (historia de usuario, criterios de aceptación y reglas de negocio), en lenguaje del negocio.
2. **`specs.md`** — cómo se resuelve técnicamente, dividido por capas (Domain, Application, Infrastructure, Presentation).
3. **`tasks.md`** — lista de tareas verificables para implementar la spec.
No comiences a codear sin haber revisado la spec y las tareas del requerimiento. Si algo no está especificado, **PREGUNTA** antes de inventarlo.

## 3. Auto-Validación Continua (Ejecución de Tests)
No asumas que tu código es perfecto. Cada vez que crees, modifiques o refactorices código (especialmente si agregas nuevas dependencias, clases, repositorios o servicios), tienes la **OBLIGACIÓN** de verificar que no has roto la arquitectura.
- Debes usar tu herramienta de terminal (ej. `run_command`) para ejecutar las pruebas automatizadas del proyecto de control de calidad.
- El comando a ejecutar es: `dotnet test` apuntando al proyecto `CodeQuality.Tests` (deberás buscar la ruta en el espacio de trabajo del usuario si no es evidente).

## 4. Auto-Corrección
Si al ejecutar `dotnet test` algún test de ArchUnitNET falla, **NO le pidas al usuario que lo arregle ni des por terminada tu tarea.**
1. Lee el mensaje de error del test fallido (que indica qué regla arquitectónica rompiste).
2. Modifica el código que generaste para solucionar el problema.
3. Vuelve a ejecutar los tests.
Solo debes reportar tu trabajo al usuario cuando todas las pruebas pasen en verde.
4. No se deben de modificar los test para que pasen sin consultar al usuario explícitamente que autorice esta modificación.

## 5. TDD y Pruebas Unitarias
- **Obligatorio para el Dominio:** Cada vez que generes o modifiques comportamiento o lógica dentro de la carpeta `Domain` (ej. métodos de entidades, validaciones, cálculos), **DEBES** generar simultáneamente la prueba unitaria (xUnit) correspondiente para validar ese comportamiento.
- **Silencio en otras capas:** Para las capas de `Application`, `Infrastructure` o `Presentation`, NO generes pruebas unitarias. Además, NO debes preguntarle ni sugerirle al usuario la creación de tests en estas capas. Mantén tus respuestas enfocadas estrictamente en lo que el usuario solicitó.

## 6. Integridad de los Guardarraíles (Prohibido Alterar Tests Estructurales)
- **Regla de Inmutabilidad:** Tienes estrictamente **PROHIBIDO** modificar los archivos del proyecto `CodeQuality.Tests` (los tests de ArchUnitNET) para lograr que una prueba pase.
- **Resolución de Conflictos:** Si un test arquitectónico falla, tu obligación es SIEMPRE corregir el código fuente del sistema para que cumpla con el test. NUNCA debes relajar ni alterar el test para adaptarlo al código mal diseñado.
- **Intervención Humana:** Si tras analizar un error crees genuinamente que la regla de ArchUnitNET está mal planteada, está desactualizada, o se requiere una excepción justificada para el diseño actual, debes **DETENERTE**. Explica la situación al usuario humano y solicita su autorización explícita antes de editar cualquier archivo de pruebas de calidad.

## 7. Análisis Estático (SonarQube)
El proyecto usa SonarQube como herramienta de análisis estático (código duplicado, bugs y code smells). Antes de dar por cerrada una funcionalidad relevante, y cuando el entorno local lo permita, ejecuta el análisis con el script correspondiente (`ControlCalidad/SonarQube/analizar.ps1` en Windows) y revisa los resultados en `http://localhost:9000`.
