# The game server (TCP 6414) and status server (TCP 6416) on Linux; docker-compose.yml runs it with PostgreSQL.
# The game data is not in the image: mount the client's data folder on /gamedata.
# The settings are in /app/config. A folder mounted there gets the default of any settings file it lacks when the server starts.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# The project files first, so the restore layer is reused while only code changes.
COPY Directory.Build.props ./
COPY src/NorthIsland.Protocol/NorthIsland.Protocol.csproj src/NorthIsland.Protocol/
COPY src/NorthIsland.GameData/NorthIsland.GameData.csproj src/NorthIsland.GameData/
COPY src/NorthIsland.Server/NorthIsland.Server.csproj src/NorthIsland.Server/
RUN dotnet restore src/NorthIsland.Server/NorthIsland.Server.csproj
COPY src/ src/
RUN dotnet publish src/NorthIsland.Server/NorthIsland.Server.csproj -c Release --no-restore -o /out

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /out .
# Writable folders for the non-root user; named volumes mounted here start with this owner. config-default keeps the image's settings
# for filling in a mounted config folder.
RUN mkdir -p /app/data /app/logs && cp -r /app/config /app/config-default && chown -R $APP_UID /app/data /app/logs /app/config
USER $APP_UID
ENV Server__GameDataDirectory=/gamedata \
    Server__DataDirectory=/app/data \
    Server__LogDirectory=/app/logs
EXPOSE 6414 6416
VOLUME ["/app/data", "/app/logs"]
ENTRYPOINT ["sh", "-c", "for f in /app/config-default/*; do [ -e \"/app/config/${f##*/}\" ] || cp \"$f\" /app/config/ || echo \"Could not copy ${f##*/} into /app/config\" >&2; done; exec dotnet NorthIsland.Server.dll \"$@\"", "sh"]
