# Especificacion Tecnica: <Nombre del requerimiento>

> Completar todas las secciones. Si una decision no esta definida, preguntar
> antes de escribir. No dejar "a definir", "evaluar" ni "segun convencion".

## Objetivo

...

## Decisiones y supuestos

- ...

## 1) Dominio (`SolarUY.Domain`)

### Entidades

...

### Validaciones

...

### Excepciones

...

### Repositorios (contratos)

...

## 2) Aplicacion (`SolarUY.Application`)

### DTOs

...

### Servicios y casos de uso

...

## 3) Infraestructura (`SolarUY.Infrastructure`)

### DbSet y configuraciones

...

### Repositorios

...

### Migracion

Indicar si hay cambios de esquema y el nombre de la migracion, o justificar por
que no se necesita.

...

### Registro de dependencias (`Program.cs`)

...

## 4) Presentacion (`SolarUY.Controllers` + `Views`)

### Controlador y acciones

...

### Autorizacion

...

### Vistas

...

## 5) Pruebas

- Tests xUnit del Dominio (obligatorios): ...
- No se agregan tests en Application, Infrastructure ni Presentation.

## 6) Calidad y verificacion

- [ ] Build: `docker compose run --rm tools dotnet build SolarUY.sln`
- [ ] Tests: `docker compose run --rm tools dotnet test SolarUY.sln`
- [ ] ArchUnitNET en verde
- [ ] SonarQube ejecutado y hallazgos revisados
- [ ] Prueba manual del flujo completo

## 7) Fuera de alcance

- ...
