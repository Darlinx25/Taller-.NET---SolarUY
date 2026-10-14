# Reglas Avanzadas: Capa de Presentación (ASP.NET Core MVC)

Estas reglas están diseñadas para garantizar que se respeten las fronteras de responsabilidad semántica de la interfaz de usuario.

## Prohibición de Lógica de Negocio en la Capa de Presentación (Controladores MVC)
- **Directiva:** Los Controladores MVC y las vistas Razor (`.cshtml`) deben ser estrictamente "Vistas Tontas" (Dumb Views). Su única responsabilidad es capturar/recibir datos, invocar casos de uso y mostrar resultados.
- **Validación Semántica:** Como agente de IA, **DEBES** rechazar o advertir firmemente si el usuario solicita o implementa lógica condicional de negocio (ej. usar un `if / switch` para determinar reglas críticas del dominio) o cálculos matemáticos (ej. iterar un `foreach` para calcular la generación total de un parque) directamente dentro de un Controlador o de una vista Razor.
- **Corrección Exigida:** Toda lógica de cálculo o decisión debe ocurrir en `Domain` o en un `Application Service`. El Controlador solo debe invocar un método en la capa de `Application` o recibir un `ViewModel` con los datos ya calculados y listos para ser renderizados.
- **Datos sensibles:** El Controlador no debe acceder directamente a `DbContext` ni a repositorios concretos de `Infrastructure`; siempre a través de un servicio de `Application`.
