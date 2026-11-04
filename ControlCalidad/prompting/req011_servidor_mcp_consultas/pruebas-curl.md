# Pruebas manuales del servidor MCP (req011)

Requisitos: `docker compose ps` con `app` healthy. El endpoint solo acepta `POST`
(un `GET` devuelve `405`). La respuesta es SSE (`event: message` + `data: {...}`).
El transporte es stateless, asi que no hace falta guardar sesion entre llamadas.

## Salud

```bash
curl -s http://127.0.0.1:5080/health
```

## Inicializar

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2025-06-18","capabilities":{},"clientInfo":{"name":"curl","version":"1.0"}}}'
```

## Listar herramientas

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}'
```

## Llamar herramientas

### listar_parques

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"listar_parques","arguments":{}}}'
```

## listar parques JSON
```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"listar_parques","arguments":{}}}' \
  | sed -n 's/^data: //p' | jq -r '.result.content[0].text' | jq .
  ```

### obtener_topologia

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"obtener_topologia","arguments":{}}}'
```

### obtener_estado_actual

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"name":"obtener_estado_actual","arguments":{}}}'
```

### obtener_historico

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":6,"method":"tools/call","params":{"name":"obtener_historico","arguments":{"tipoEntidad":"Parque","desde":"2026-11-01","hasta":"2026-11-04","agrupacion":"Dia"}}}'
```

### obtener_alertas

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":7,"method":"tools/call","params":{"name":"obtener_alertas","arguments":{}}}'
```

### listar_tareas_mantenimiento (sin filtro)

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":8,"method":"tools/call","params":{"name":"listar_tareas_mantenimiento","arguments":{}}}'
```

### listar_tareas_mantenimiento (por estado)

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":9,"method":"tools/call","params":{"name":"listar_tareas_mantenimiento","arguments":{"estado":"Pendiente"}}}'
```

## Probar un error controlado

Debe devolver `isError: true` con un mensaje en espanol.

```bash
curl -s http://127.0.0.1:5080/mcp \
  -H 'Content-Type: application/json' \
  -H 'Accept: application/json, text/event-stream' \
  -d '{"jsonrpc":"2.0","id":10,"method":"tools/call","params":{"name":"obtener_historico","arguments":{"tipoEntidad":"Parque","desde":"2026-11-04","hasta":"2026-11-01","agrupacion":"Dia"}}}'
```
