# Convenciones de Nomenclatura en .NET (SolarUY - MVC monolítico en capas)

Al momento de generar código, refactorizar o proponer soluciones, el agente debe respetar de manera estricta las convenciones de nomenclatura estándar de C# y del proyecto, con el objetivo de mantener un código legible, uniforme y predecible. Estas convenciones son evaluadas mediante pruebas automatizadas con ArchUnitNET.

## 1. Reglas Generales de C#
- **Interfaces:** Toda interfaz debe comenzar invariablemente con la letra `I` mayúscula (ej. `IParqueSolarRepository`, `INotificacionService`).
- **Excepciones:** Cualquier clase personalizada que herede de `System.Exception` debe terminar con el sufijo `Exception` (ej. `NombreParqueDuplicadoException`).
- **PascalCase vs camelCase:**
  - `PascalCase`: Se utiliza para nombres de Clases, Records, Interfaces, Métodos y Propiedades públicas.
  - `camelCase`: Se utiliza para variables locales y parámetros de métodos.
  - `_camelCase` (con guion bajo): Se utiliza EXCLUSIVAMENTE para variables de instancia privadas (fields), en especial aquellas inyectadas por dependencia en el constructor (ej. `private readonly IParqueSolarRepository _parqueSolarRepository;`).

## 2. Reglas de Capas (monolito MVC en capas)
- **Repositorios (Infrastructure):** Toda clase que gestione el acceso a datos debe tener el sufijo `Repository`. Nunca debe usarse "Dao" o "GestorDatos". Su abstracción correspondiente debe llamarse igual, prefijada con I (ej. `IParqueSolarRepository` → `ParqueSolarRepository`).
- **Servicios (Application):** Las clases que coordinan la lógica de negocio o actúan como Casos de Uso deben terminar con el sufijo `Service` (ej. `ParqueSolarService`).
- **Controladores MVC (Presentation):** Las clases controladoras de ASP.NET Core MVC deben terminar en `Controller` (ej. `ParquesSolaresController`) y ubicarse en la carpeta `Controllers/`.
- **Vistas Razor (Presentation):** Los archivos `.cshtml` se nombran igual que la acción del controlador y viven en `Views/<Controlador>/` (ej. `Views/ParquesSolares/Create.cshtml` para `ParquesSolaresController.Create`).
- **ViewModels / DTOs:** Los modelos que viajan entre capas o hacia las vistas terminan en `Dto` o `ViewModel` según su uso (ej. `CrearParqueSolarDto`, `ParqueSolarViewModel`).

## 3. Acrónimos
Si se utilizan acrónimos de dos letras (ej. ID, IP), ambas deben ir en mayúscula si es la primera palabra (ej. `IDParque`) o PascalCase si están en medio (ej. `ParqueId`). Los acrónimos de más de dos letras deben tratarse como palabras normales (ej. `XmlDocument`, no `XMLDocument`).

## 4. Idioma (Spanglish Técnico)
- **Dominio en Español:** Los nombres base de las entidades, variables, propiedades y métodos que representan reglas de negocio deben escribirse en **Español** (ej. `ParqueSolar`, `Nodo`, `Panel`, `CalcularGeneracion()`, `_nodoActivo`).
- **Sufijos Estructurales en Inglés:** Debes respetar obligatoriamente los sufijos y prefijos técnicos en **Inglés** exigidos en las secciones anteriores. **No debes traducirlos**. (Ejemplo correcto: `ParqueSolarRepository`, `NodoService`, `InversorNoDisponibleException`, `IPanelRepository`).
