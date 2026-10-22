FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY SolarUY/SolarUY.csproj SolarUY/
RUN dotnet restore SolarUY/SolarUY.csproj
COPY SolarUY/ SolarUY/
RUN dotnet publish SolarUY/SolarUY.csproj -c Release -o /publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=build /publish .

USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SolarUY.dll"]
