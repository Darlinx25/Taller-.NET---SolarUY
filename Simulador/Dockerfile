FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

RUN dotnet workload install wasm-tools

WORKDIR /src
COPY src/SolarUY.Simulador/SolarUY.Simulador.csproj src/SolarUY.Simulador/
COPY src/SolarUY.Simulador.Wasm/SolarUY.Simulador.Wasm.csproj src/SolarUY.Simulador.Wasm/
RUN dotnet restore src/SolarUY.Simulador.Wasm/SolarUY.Simulador.Wasm.csproj
COPY src/ src/
RUN dotnet publish src/SolarUY.Simulador.Wasm/SolarUY.Simulador.Wasm.csproj -c Release --no-restore

FROM node:22-alpine

WORKDIR /app
COPY --from=build --chown=node:node /src/src/SolarUY.Simulador.Wasm/bin/Release/net10.0/browser-wasm/AppBundle/ .

USER node

CMD ["node", "main.mjs"]
