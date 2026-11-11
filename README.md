# North Island Server

A server emulator for IGG's Wonderland Online 6.1.9 client, written in C# / .NET 10. It is built to work with the north-island-client, but it should work with the original `alogin.exe` that comes with the original IGG Wonderland Online 6.1.9 Installer as well.

## Layout

| path | what |
|---|---|
| `src/NorthIsland.Protocol` | framing, packet reader and writer, packet layouts |
| `src/NorthIsland.GameData` | readers for the client's data files |
| `src/NorthIsland.Server` | game server (TCP 6414) and status server (TCP 6416) |

Builds go to `out/bin/x64` (or `x32`, `arm64`, `etc`).

## Build & Run

You need the .NET 10 SDK and the data files of IGG's Wonderland Online 2014 client (version `6.1.9`, dated `10/27/2014`).

```bash
dotnet run --project src/NorthIsland.Server
```

### Portable build

This makes a single program that runs on 64-bit Windows without .NET installed:

```bash
dotnet publish src/NorthIsland.Server/NorthIsland.Server.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o out/publish/win-x64
```

Copy the `out/publish/win-x64` folder anywhere and run `NorthIsland.Server.exe`. Keep the `config` folder next to it,
because that holds its settings. Once the folder is outside this one, `data/` and `logs/` are created
next to the program.

## Game data

The server looks for the client's data files in this order and stops with an error if it finds none:

1. the folder given with `--data`
2. the `Server__GameDataDirectory` environment variable, then `Server:GameDataDirectory` in `config/appsettings.json`
3. the folder of the server program, then this folder
4. `Program Files (x86)\Wonderland Online` on Windows

A folder may be the game's folder or its `data` folder. A relative path starts from this folder.

```bash
dotnet run --project src/NorthIsland.Server -- --data "C:\Games\Wonderland Online"
```

## Settings and saved data

Every settings file is in `src/NorthIsland.Server/config/`. The essentials and simple modifiers are in `appsettings.json`,
and the rest of the tuning is in `battle.json`, `progression.json`, `world.json` and `mall.json`. The event calendar is
`events.json` and the Q&A questions are `quiz.json`. What each setting does is in the `SETTINGS.md` file in the same folder.

Accounts are written to `data/accounts/<name>.json` and the log to `logs/server-yyyyMMdd.log`. Unknown accounts are
created on first login (`AutoRegister`).

Accounts and the world's other saved state can be kept in PostgreSQL instead, with `Database__Provider=Postgres` and
the connection string in `ConnectionStrings__Accounts`.

## Docker
```bash
docker compose up -d --build
```

This runs the server with a PostgreSQL container. Put the client's `data` folder in `gamedata/`, or point at it with
`GAMEDATA` in a `.env` file next to `docker-compose.yml`. `POSTGRES_PASSWORD` is required in that file, and Docker will
not start without it. Set `POSTGRES_PORT` there too if 5432 is taken.

The settings are built into the image. To change them without rebuilding, mount a folder on `/app/config`:

```yaml
    volumes:
      - ./config:/app/config
```

When the server starts, it copies the default of any settings file the folder lacks, so an empty folder is filled with
all of them. The folder must be writable by the user the container runs as. Files already there are left alone, so after
an update new settings in a file you keep only take effect once you add them to it. Restart the server after editing.

## Point the client at it

Work on a copy of the game folder and replace its `SERVER.INI` with:

```
01[Local]1
Local*127.0.0.1
```

Start the client, choose the "Local" group and server, and log in with any 4 to 14 character alphanumeric ID and a password.

## GM commands

### Make the first GM

The easiest way is the `Server__GmAccounts` environment variable: a list of account names, separated by commas. Those
accounts are made GMs the next time they log in. With Docker, put this in the `.env` file next to `docker-compose.yml`
and run `docker compose up -d` again:

```
GM_ACCOUNTS=youraccount
```

Without Docker, set `Server__GmAccounts` before starting the server, or set `GmAccounts` in `config/appsettings.json`. Taking a
name off the list does not take GM away; use `/deop` for that.

You can also set it by hand. The server reads accounts when it starts and writes them back while it runs, so make the
change while it is stopped.

1. Log in once with the account you want to use, so that it exists.
2. Stop the server.
3. Open `data/accounts/<name>.json`, with the account name in lower case, and add this line right after the opening `{`:

   ```json
   "Gm": true,
   ```

4. Start the server, log in and type `/help` in the chat.

With Docker the accounts are in PostgreSQL. Run these from this folder, using your account name:

```bash
docker compose stop server
docker compose exec db psql -U northisland -d northisland -c "UPDATE accounts SET data = jsonb_set(data, '{Gm}', 'true') WHERE name_key = lower('YourAccount');"
docker compose start server
```

The second command should print `UPDATE 1`. After that, use `/op` in the game to make more GMs.

### Commands

Type these in the chat. They are not sent to other players. `<name>` can be a character name or an account name.

| Command | What it does |
|---|---|
| `/help` | Lists the commands |
| `/op <name>` | Makes an account a GM |
| `/deop <name>` | Takes GM away from an account; you cannot use it on yourself |
| `/kick <name>` | Disconnects an online player |
| `/ban <name>` | Bans an account and disconnects it; you cannot ban yourself |
| `/ban-ip <name>` | Also bans the player's address; the player must be online, and your own or a local address is refused |
| `/unban <name or address>` | Lifts the bans of a name or address |
| `/gamemode admin` | You take no damage, and the admin item browser opens in the North Island client |
| `/gamemode normal` | Turns admin mode off |
| `/give <item id> [count]` | Puts an item in your own bag; the count is 1 to 999 |
| `/level <level> [name]` | Sets your level or an online player's; not during a battle |
| `/tp <map> <x> <y>` | Moves you to a position |
| `/tp <name>` | Moves you to an online player |
| `/tp <name> <map> <x> <y>` | Moves an online player to a position |
| `/tp <name> <other name>` | Moves one online player to another |
| `/tphere <name>` | Brings an online player to you |

## Disclaimer

North Island Client is an unofficial, non-commercial fan project. It is not affiliated with, endorsed by, or supported by IGG or any other owner of Wonderland Online.

Wonderland Online, its name, game data, artwork, music, and other assets are the property of their respective owners. None of the game's data is included in this repository, and you must supply your own copy of the game files. The license for this project does not apply to those assets or grant any rights to them.

This project was developed in part by reverse engineering the original client. The license applies only to the original work of this project's contributors.

This software is provided "as is", without warranty of any kind. Use it at your own risk.

## Pre-alpha Testing

Though the server should be fully functional as is. The server's code is currently being play-tested by the developers through a normal playthrough of the game to find any issues or mismatches than what is expected. The most common features have already been tested, but group activites and events may have some edge cases not covered yet.
