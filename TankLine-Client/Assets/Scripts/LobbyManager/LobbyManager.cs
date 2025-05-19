using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Observing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AuthController))]
[RequireComponent(typeof(NetworkObserver))]
[RequireComponent(typeof(PlayerSpawner))]
public class LobbyManager : NetworkBehaviour
{

    void Awake()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneChangement;
        gameObject.GetComponent<NetworkObject>().SetIsGlobal(true);

        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") != "true") return;

        // find networkManager
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError($"[ERROR] NetworkManager is null in waiting room");
            return;
        }

        // on client connexion change
        networkManager.ServerManager.OnRemoteConnectionState += (connection, state) =>
        {
            // if client is disconnecting
            if (state.ConnectionState == FishNet.Transporting.RemoteConnectionState.Stopped)
            {
                // remove from list
                RemovePlayerFromList_ServerSide(connection);
            }
        };
    }

    void Start()
    {
        // initialize everything
        playerSpawner = gameObject.GetComponent<PlayerSpawner>();
        uiManager = new(GameObject.Find("Canvas"), CurrentSceneName() == "LoadScene");
        ChangeSpawnPrefab(CurrentSceneName());
        playerSpawner.InitSpawnPoint();
        uiManager.CreateSkinsPanel(skins, skinEntryPrefab);

        // load skins
        for (int i = 0; i < skins.Count; i++)
        {
            availableSkins.Add(i);
        }

        // send username to DB
        if (base.IsClientInitialized)
        {
            authController = gameObject.GetComponent<AuthController>();
            StartCoroutine(SendPlayerName());
        }
    }






    /*
        ############################################################
                            Spawner management
        ############################################################
    */

    private PlayerSpawner playerSpawner;
    private int nbPlayerReady = 0;

    [Space(15)]
    [Header("Player Prefabs")]
    [Space(5)]
    public GameObject WaitingRoomTankPrefab;
    public GameObject InGameTankPrefab;
    private IEnumerator loadingTimeoutCoroutine;
    private bool hasGameStarted = false;

    public void ChangeSpawnPrefab(string sceneName)
    {
        if (playerSpawner == null)
        {
            Debug.Log("[ERROR] : PlayerSpawner null in ChangeSpawnPrefab");
            return;
        }
        switch (sceneName)
        {
            case "LoadScene":
                playerSpawner.playerObjectPrefab = InGameTankPrefab;
                break;
            case "WaitingRoom":
                playerSpawner.playerObjectPrefab = WaitingRoomTankPrefab;
                break;

            default:
                Debug.LogError("Scene not found for ChangeSpawnPrefab");
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void NewPlayerReady()
    {
        nbPlayerReady++;
        Debug.Log($"[In-Game] Waiting players : {nbPlayerReady}/{serverPlayerList.Count}");

        // if everyone ready, spawn everyone
        if (nbPlayerReady >= serverPlayerList.Count)
        {
            hasAutoReplayStarted = false;
            StartGame();
        }
    }

    public void IsReady()
    {
        playerSpawner.InitSpawnPoint();
        NewPlayerReady();
    }

    public void OnPlayerSpawned()
    {
        SyncTexture(PlayerData.GetSkinsDic(serverPlayerList));
    }

    public void StartGame()
    {
        alivePlayers = serverPlayerList.Values.ToList();
        OnPlayerListChange(alivePlayers); // send names to update UI
        Debug.Log("[In-Game] Starting game... Spawning player");
        hasGameStarted = true;

        foreach (KeyValuePair<NetworkConnection, PlayerData> entry in serverPlayerList)
        {
            playerSpawner.SpawnPlayer(entry.Key, entry.Value.name);
        }
    }

    private IEnumerator StartLoadingTimeoutCoroutine(int nbSeconds)
    {
        yield return new WaitForSeconds(nbSeconds);

        if (!hasGameStarted)
            StartGame();
    }

    [ServerRpc(RequireOwnership = false)]
    private void JoinedWaitingRoom(NetworkConnection connection = null)
    {
        PlayerData player = serverPlayerList[connection];

        if (player == null)
        {
            Debug.Log("[ERROR] Player joining waiting room from previous game is null");
            return;
        }

        Debug.Log($"[Waiting room] Player {player.name} joining from previous game. Spawning character...");
        playerSpawner.SpawnPlayer(connection, player.name);

        OnPlayerListChange(serverPlayerList.Values.ToList());
    }








    /*
        ############################################################
                               Scene management
        ############################################################
    */

    public string CurrentSceneName()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void LaunchGame_Server()
    {
        // Load new scene
        SceneLoadData newScene = new("LoadScene");
        newScene.ReplaceScenes = ReplaceOption.All; // destroy every other object
        newScene.MovedNetworkObjects = new NetworkObject[] { this.GetComponent<NetworkObject>() }; // keep this object in the next scene

        Debug.Log($"[Lobby #number] Launching game");

        // change scene
        InstanceFinder.SceneManager.LoadGlobalScenes(newScene);
    }

    public void ClickedStartGame()
    {
        Debug.Log("[CLIENT] Launching game");
        LaunchGame_Server();
    }

    private void OnSceneChangement(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneChangement;
            Debug.Log("MainMenu scene : destroying networkManager & LobbyManager");
            NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
            Destroy(networkManager);
            Destroy(gameObject);
            return;
        }

        // init new spawn points
        playerSpawner = gameObject.GetComponent<PlayerSpawner>();
        ChangeSpawnPrefab(scene.name);
        playerSpawner.InitSpawnPoint();

        // init new ui manager
        uiManager = new(GameObject.Find("Canvas"), scene.name == "LoadScene");
        hasAutoReplayStarted = false;
        hasGameStarted = false;
        playerReadyForReplay = new();
        playerScores.Clear();
        foreach (var kvp in serverPlayerList)
        {
            playerScores.Add(kvp.Key, 0);
        }

        // if in game
        if (scene.name == "LoadScene")
        {
            // reset the number of player ready when reloading a new game
            nbPlayerReady = 0;

            // set a timeout to launch game even if a player can't make it in game
            if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") == "true")
            {
                loadingTimeoutCoroutine = StartLoadingTimeoutCoroutine(5);
                StartCoroutine(loadingTimeoutCoroutine);
            }
        }
        else

        // if in waiting room
        if (scene.name == "WaitingRoom")
        {

            // if is running on server
            if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") == "true")
            {
                // send player list to update ui
                OnPlayerListChange(serverPlayerList.Values.ToList());

                ReplayClient();
            }

            // if on the client
            else
            {
                JoinedWaitingRoom(base.ClientManager.Connection);
                uiManager.CreateSkinsPanel(skins, skinEntryPrefab);
                GetAvailableSkins();
            }
        }
    }

    [ObserversRpc]
    private void ReplayClient()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("WaitingRoom");
    }

    private void ClearAndReplayServer()
    {
        nbPlayerReady++;

        // reset number of lives
        foreach (PlayerData player in serverPlayerList.Values.ToList())
        {
            player.livesLeft = maxPlayerLives;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("WaitingRoom");
    }

    private IEnumerator AutoReplayCoroutine(int nbSeconds)
    {
        yield return new WaitForSeconds(nbSeconds);
        if (hasAutoReplayStarted)
            ClearAndReplayServer();
    }






    /*
        ############################################################
                            Player list management
        ############################################################
    */

    private readonly Dictionary<NetworkConnection, PlayerData> serverPlayerList = new();
    private List<PlayerData> clientPlayerList = new();


    private IEnumerator SendPlayerName()
    {
        // fetch user data
        yield return authController.User();

        if (authController.CurrentUser == null)
        {
            Debug.LogError("[ERROR] Current user is null, cannot send data");
        }
        else
        {
            // send username to server
            string userName = authController.CurrentUser.username.ToString();
            Debug.Log("connecting...");
            AddPlayerToList(base.ClientManager.Connection, userName);
        }
    }

    /// <summary>
    /// Add a player to the server-side playerList
    /// </summary>
    /// <param name="name">The username to add</param>
    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerToList(NetworkConnection connection, string name)
    {
        Debug.Log($"[Lobby #number] New player connected : {name}");

        // add player to list
        serverPlayerList.Add(connection, new(name, maxPlayerLives, availableSkins[0]));
        OnPlayerListChange(serverPlayerList.Values.ToList());

        // spawn player object if in waiting room
        if (CurrentSceneName() == "WaitingRoom")
            playerSpawner.SpawnPlayer(connection, name);

        // Remove the skin given to new player from availables ones
        availableSkins.RemoveAt(0);
    }

    /// <summary>
    /// Add a player to the server-side playerList (call only from client-side)
    /// </summary>
    /// <param name="connection">Auto-fill by fishnet, the connection of the client who calls this procedure</param>
    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient(NetworkConnection connection = null)
    {
        if (connection == null) return;

        Debug.Log($"[Lobby #number] Disconnecting player : {serverPlayerList[connection].name}");

        // Break client connection
        connection.Disconnect(true);
    }

    /// <summary>
    /// Remove a player to the server-side playerList (call only from server-side)
    /// </summary>
    /// <param name="connection">The connection to remove</param>
    private void RemovePlayerFromList_ServerSide(NetworkConnection connection)
    {
        Debug.Log($"[Lobby #number] Removing player from list : {serverPlayerList[connection].name}");

        // despawn player
        PlayerData leavingPlayer = serverPlayerList[connection];
        playerSpawner.DespawnPlayer(leavingPlayer.name);

        // Add leaving player skins to available ones
        availableSkins.Add(leavingPlayer.skinColor);

        if (alivePlayers.Exists(player => player.name == serverPlayerList[connection].name))
            alivePlayers.Remove(serverPlayerList[connection]);

        // if autoreplay has started, remove the player from the ReadyForReplay
        if (hasAutoReplayStarted)
        {
            if (playerReadyForReplay.Exists(player => player.name == serverPlayerList[connection].name))
            {
                playerReadyForReplay.Remove(serverPlayerList[connection]);
            }
        }

        // if only one player is alive, end game
        if (alivePlayers.Count() == 1)
        {
            Debug.Log($"[In-Game] End of game. {alivePlayers[0].name} won !");
            ShowEndGamePanel(alivePlayers[0].name);
        }

        // remove player from list
        serverPlayerList.Remove(connection);

        if (CurrentSceneName() == "LoadScene")
        {
            OnPlayerListChange(alivePlayers);
        }
        else
        {
            OnPlayerListChange(serverPlayerList.Values.ToList());
        }

        // if this was the last player in game, return to waiting room
        if (CurrentSceneName() == "LoadScene" && serverPlayerList.Count() == 0)
        {
            Debug.Log("[In Game] No more player in game, returning to waiting room");
            ClearAndReplayServer();
        }

        if (hasAutoReplayStarted)
        {
            // update the ui
            UpdateReadyForReplay(serverPlayerList.Count, playerReadyForReplay.Count);

            // check if every remaining players are ready
            if (playerReadyForReplay.Count == serverPlayerList.Count)
            {
                ClearAndReplayServer();
            }
        }
    }

    /// <summary>
    /// Function called by the server when the player list is actualized
    /// </summary>
    /// <param name="newNames">The new player list sent by the server</param>
    [ObserversRpc]
    private void OnPlayerListChange(List<PlayerData> newNames)
    {
        clientPlayerList = newNames;

        uiManager.UpdateUI(clientPlayerList, playerEntryPrefab, minimumPlayerToStart);
    }






    /*
        ############################################################
                                  UI Manager 
        ############################################################
    */
    [Space(15)]
    [Header("UI References")]
    [Space(5)]
    public GameObject playerEntryPrefab;
    public GameObject skinEntryPrefab;
    [Range(1, 6)]
    public int minimumPlayerToStart = 2;
    private InGameUiManager uiManager;
    private AuthController authController;
    bool hasAutoReplayStarted = false;

    [TargetRpc]
    private void StartRespawnCountdown(NetworkConnection conn)
    {
        StartCoroutine(uiManager.RespawnCountdownCoroutine(respawnCooldown));
    }

    [TargetRpc]
    private void SetLife(NetworkConnection conn, int newlife)
    {
        uiManager.SetLifeUI(newlife);
    }

    [TargetRpc]
    private void ShowDefeatPanel(NetworkConnection conn)
    {
        uiManager.ShowDefeatPanel();
    }

    [ObserversRpc]
    private void ShowEndGamePanel(string winnerName)
    {
        uiManager.ShowEndGamePanel(winnerName, winnerName == authController.CurrentUser.username.ToString());
    }

    [ObserversRpc]
    private void UpdateReadyForReplay(int nbPlayer, int nbReady)
    {

        if (CurrentSceneName() == "WaitingRoom") return;

        if (!hasAutoReplayStarted)
        {
            hasAutoReplayStarted = true;
            StartCoroutine(uiManager.StartAutoReplayCountdown(autoReplayCountdown));
        }

        uiManager.UpdateReadyForReplay(nbPlayer, nbReady);
    }

    public void SendReadyForReplay()
    {
        ReadyForReplay();
    }






    /*
        ############################################################
                                Game Logic
        ############################################################
    */

    [Space(15)]
    [Header("Game parameters")]
    [Space(5)]

    [Range(1, 5)]
    public int maxPlayerLives = 3;
    [Range(1, 5)]
    public int respawnCooldown = 3;
    [Range(5, 30)]
    public int autoReplayCountdown = 15;
    public List<Material> skins = new();
    private List<int> availableSkins = new();
    private List<PlayerData> alivePlayers = new();
    private List<PlayerData> playerReadyForReplay = new();
    private Dictionary<NetworkConnection, int> playerScores = new();

    [ObserversRpc]
    private void SyncTexture(Dictionary<NetworkConnection, int> playersSkins)
    {

        // in game players
        if (CurrentSceneName() == "LoadScene")
        {
            Tank_Player[] playersInGame = FindObjectsByType<Tank_Player>(FindObjectsSortMode.None);
            foreach (Tank_Player player in playersInGame)
            {
                GameObject go = player.GameObject();
                Material skinColor = skins[playersSkins[go.GetComponent<NetworkObject>().Owner]];

                Material[] mat = go.transform.Find("base").GetComponent<Renderer>().materials;
                mat[4] = skinColor;
                mat[5] = skinColor;
                go.transform.Find("base").GetComponent<Renderer>().SetMaterials(mat.ToList());
                mat = go.transform.Find("tankGun").GetComponent<Renderer>().materials;
                mat[0] = skinColor;
                mat[1] = skinColor;
                go.transform.Find("tankGun").GetComponent<Renderer>().SetMaterials(mat.ToList());
            }
        }

        else
        {
            // Lobby players
            Tank_Lobby[] playersLobby = FindObjectsByType<Tank_Lobby>(FindObjectsSortMode.None);
            foreach (Tank_Lobby player in playersLobby)
            {
                GameObject go = player.GameObject();
                Material skinColor = skins[playersSkins[go.GetComponent<NetworkObject>().Owner]];

                Material[] mat = go.transform.Find("base").GetComponent<Renderer>().materials;
                mat[4] = skinColor;
                mat[5] = skinColor;
                go.transform.Find("base").GetComponent<Renderer>().SetMaterials(mat.ToList());
                mat = go.transform.Find("tankGun").GetComponent<Renderer>().materials;
                mat[0] = skinColor;
                mat[1] = skinColor;
                go.transform.Find("tankGun").GetComponent<Renderer>().SetMaterials(mat.ToList());
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateAvailableSkins(NetworkConnection conn = null)
    {
        ClientUpdateAvailableSkins(conn, availableSkins);
    }

    [ObserversRpc]
    [TargetRpc]
    private void ClientUpdateAvailableSkins(NetworkConnection target, List<int> serverAvailableSkins)
    {
        availableSkins = serverAvailableSkins;
        uiManager.UpdateSkinsPanel(availableSkins);
    }

    public void GetAvailableSkins()
    {
        UpdateAvailableSkins(base.ClientManager.Connection);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeSkin(int newSkin, NetworkConnection conn = null)
    {
        if (!availableSkins.Contains(newSkin)) return;

        availableSkins.Add(serverPlayerList[conn].skinColor);
        serverPlayerList[conn].skinColor = newSkin;
        availableSkins.Remove(newSkin);
        ClientUpdateAvailableSkins(null, availableSkins);
        SyncTexture(PlayerData.GetSkinsDic(serverPlayerList));
    }

    public void PickSkin(int skinIndex)
    {
        ChangeSkin(skinIndex, base.ClientManager.Connection);
        GetAvailableSkins();
    }

    public void OnPlayerHit(NetworkConnection connection, NetworkConnection attacker)
    {
        // connection is the player who got hit
        try
        {
            serverPlayerList[connection].livesLeft--; // lose a life

            // despawn player
            playerSpawner.DespawnPlayer(serverPlayerList[connection].name);
            // set UI player life 
            SetLife(connection, serverPlayerList[connection].livesLeft);

            // if life left, respawn after a short duration
            if (serverPlayerList[connection].livesLeft > 0)
            {
                if (attacker != connection)
                {
                    playerScores[attacker]++;
                    Debug.Log($"[In-Game] Player {serverPlayerList[attacker].name} score: {playerScores[attacker]} points");
                }
                Debug.Log($"[In-Game] Player {serverPlayerList[connection].name} hit. {serverPlayerList[connection].livesLeft} lives left, respawning in {respawnCooldown} secondes");

                StartRespawnCountdown(connection); // send the message to the player to update their UI

                StartCoroutine(RespawnCoroutine(connection, serverPlayerList[connection]));
            }

            // if no life left, remove from the list alivePlayer
            else
            {
                if (attacker != connection)
                {
                    playerScores[attacker] += 2;
                    Debug.Log($"[In-Game] Player {serverPlayerList[attacker].name} score: {playerScores[attacker]} points");
                }
                Debug.Log($"[In-Game] Player {serverPlayerList[connection].name} hit. No life left, game over");
                SendScoresToDatabase(connection);
                alivePlayers.Remove(serverPlayerList[connection]);
                OnPlayerListChange(alivePlayers); // update the list for every players


                // if only one player is alive, end game
                if (alivePlayers.Count() == 1)
                {
                    Debug.Log($"[In-Game] End of game. {alivePlayers[0].name} won !");
                    ShowEndGamePanel(alivePlayers[0].name);
                    SendScoresToDatabase(serverPlayerList.FirstOrDefault(x => x.Value.name == alivePlayers[0].name).Key); // send the score to the database
                }
                else
                {
                    ShowDefeatPanel(connection);
                }
            }

        }
        catch
        {
            Debug.Log($"[ERROR] No player found for connection {connection}");
        }
    }

    private IEnumerator RespawnCoroutine(NetworkConnection playerConnection, PlayerData playerData)
    {

        // wait `respawnCooldown` seconds
        yield return new WaitForSeconds(respawnCooldown);

        // respawn player
        playerSpawner.SpawnPlayer(playerConnection, playerData.name);
        SyncTexture(PlayerData.GetSkinsDic(serverPlayerList));
        Debug.Log($"Respawning player {playerData.name}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReadyForReplay(NetworkConnection conn = null)
    {
        // if first to hit replay, start autoReplay countdown
        if (!hasAutoReplayStarted)
        {
            Debug.Log($"[Lobby #number] Starting auto-replay in {autoReplayCountdown} seconds");
            hasAutoReplayStarted = true;
            StartCoroutine(AutoReplayCoroutine(autoReplayCountdown));
        }

        // check if player already ready
        if (conn != null && !playerReadyForReplay.Exists(player => player.name == serverPlayerList[conn].name))
        {

            playerReadyForReplay.Add(serverPlayerList[conn]);
            Debug.Log($"[Lobby #number] New player ready for replay. {playerReadyForReplay.Count}/{serverPlayerList.Count}");

            // if everyone is ready, restart the game
            UpdateReadyForReplay(serverPlayerList.Count, playerReadyForReplay.Count);
            if (playerReadyForReplay.Count == serverPlayerList.Count)
            {
                ClearAndReplayServer();
            }
        }
    }

    [TargetRpc]
    private void SendScoresToDatabase(NetworkConnection conn)
    {
        authController.UpdateUserStatistics(playerScores[conn]);
    }
}

[Serializable]
public class PlayerData
{
    public string name { get; set; }
    public int livesLeft { get; set; }
    public int skinColor { get; set; }

    public PlayerData()
    {
        name = "";
        livesLeft = -1;
        skinColor = -1;
    }

    public PlayerData(string _name, int _livesLeft, int _skinColor)
    {
        name = _name;
        livesLeft = _livesLeft;
        skinColor = _skinColor;
    }

    static public List<string> GetNameList(List<PlayerData> playerDatas)
    {
        List<string> names = new();

        foreach (PlayerData player in playerDatas)
        {
            names.Add(player.name);
        }

        return names;
    }

    static public Dictionary<NetworkConnection, int> GetSkinsDic(Dictionary<NetworkConnection, PlayerData> players)
    {
        Dictionary<NetworkConnection, int> skins = new();

        foreach (KeyValuePair<NetworkConnection, PlayerData> pair in players)
        {
            skins.Add(pair.Key, pair.Value.skinColor);
        }

        return skins;
    }
}