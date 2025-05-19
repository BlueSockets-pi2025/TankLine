# TankLine - Projet Intégrateur 2025


## Description
A 2025 remake of "Tank 1990: Battle Defense War", as part of end of bachelor's 3rd year integrative project.

## Table of contents
[[_TOC_]]


## Run on university's VM

### Server 0
The server 0 is used for the unity headless server. A daemon service is already set up.  
To reload the daemon service on a newer version, you have to replace the directory `HeadlessV2` at the home directory, with your unity build inside (named `HeadlessServer.x86_64`). Once the directory is replaced, you can call the alias `restartServer` to reload the daemon service, or run the script `~/.restartServer.sh`.  
The script will tell you if the server has correctly restarted.  

**architecture :**
```
home
  ├ .restartServer.sh*
  └ HeadlessV2/
      ├ HeadlessServer.x86_64
      ├ UnityPlayer.so
      ├ HeadlessServer_Data/
      └ ...
```

### Server 1
The server 1 is the database server. It is not run by default, so if you want to connect with a client, you will first need to run the executable on this server.  
The dotnet project is currently located at `~/Tankline/Tankline-Server-1-Database/GameApi/GameApi.csproj`. You can either go to that directory and execute the command `dotnet run`, or use the alias `runServer`.

## Manual build
If you want to build your own TankLine! version, you need to change the parameters in the `TankLine-Client/Assets/StreamingAssets/.env` file (a template is available at `TankLine-Client/Assets/StreamingAssets/env.template`).

### Unity server
First you'll need to open the port the server will be listening to (the port specified in the `GAME_SERVER_PORT` var in the `.env` file).  
Then you just need to run the executable of the headless server build from unity with the environnement variable `IS_DEDICATED_SERVER` to `true`. (the command should look like this `IS_DEDICATED_SERVER=true ./HeadlessServer.X86_64`).

### Database server
The dotnet project will automatically open the ports needed, so you just need to run the executable the same way as explicited in the `Run on university's VM` > `Server 1` section.

## Authors and acknowledgment
BlueSocket

- FALCH Maëlle
- GALLONE Thibaut
- GAUCHARD Baptiste
- GAUTHERON Antoine
- HERDOIZA FUSTILLOS Jeronimo
- WAKIM Carine
- ZAIED Rayen
