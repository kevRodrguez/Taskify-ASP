FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Se copia primero el csproj para que la capa de restore se reutilice
# mientras no cambien las dependencias.
COPY Taskify.csproj ./
RUN dotnet restore Taskify.csproj

COPY . .
RUN dotnet publish Taskify.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Las imagenes de .NET corren como usuario no root. El volumen montado en /keys
# debe ser escribible por este UID o Data Protection no podra persistir el key ring.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Taskify.dll"]
