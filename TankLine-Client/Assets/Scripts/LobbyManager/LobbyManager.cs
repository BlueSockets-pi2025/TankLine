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

    void Awake() {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneChangement;
        gameObject.GetComponent<NetworkObject>().SetIsGlobal(true);

        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") != "true") return;

        // find networkManager
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null) {
            Debug.LogError($"[ERROR] NetworkManager is null in waiting room");
            return;
        }

        // on client connexion change
        networkManager.ServerManager.OnRemoteConnectionState += (connexion, state) => {
            // if client is disconnecting
            if (state.ConnectionState == FishNet.Transporting.RemoteConnectionState.Stopped) {
                // remove from list
                RemovePlayerFromList_ServerSide(connexion);
            }
        };
    }

    void Start() {
        playerSpawner = gameObject.GetComponent<PlayerSpawner>();
        uiManager = new(GameObject.Find("Canvas"), CurrentSceneName() == "LoadScene");
        ChangeSpawnPrefab(CurrentSceneName());
        playerSpawner.InitSpawnPoint();

        // load skins
        for (int i=0; i<skins.Count; i++) {
            availableSkins.Add(i);
        }

        // send username to DB
        if (base.IsClientInitialized) {
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

    public void ChangeSpawnPrefab(string sceneName) {
        if (playerSpawner == null) {
            Debug.Log("[ERROR] : PlayerSpawner null in ChangeSpawnPrefab");
            return;
        }
        switch (sceneName) {
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

    [ServerRpc(RequireOwnership=false)]
    private void NewPlayerReady() {
        nbPlayerReady++;
        Debug.Log($"[In-Game] Waiting players : {nbPlayerReady}/{serverPlayerList.Count}");

        // if everyone ready, spawn everyone
        if (nbPlayerReady >= serverPlayerList.Count) {
            OnPlayerListChange(PlayerData.GetNameList(serverPlayerList.Values.ToList())); // send names to update UI
            Debug.Log("[In-Game] Starting game... Spawning player");

            foreach (KeyValuePair<NetworkConnection, PlayerData> entry in serverPlayerList) {
                playerSpawner.SpawnPlayer(entry.Key, entry.Value.name);
            }
        }
    }

    public void IsReady() {
        playerSpawner.InitSpawnPoint();
        NewPlayerReady();
    }

    public void OnPlayerSpawned() {
        SyncTexture(PlayerData.GetSkinsDic(serverPlayerList));
    }






    /*
        ############################################################
                               Scene management
        ############################################################
    */

    public string CurrentSceneName() {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    [ServerRpc(RequireOwnership = false)]
    public void LaunchGame_Server()
    {
        // Load new scene
        SceneLoadData newScene = new("LoadScene");
        newScene.ReplaceScenes = ReplaceOption.All; // destroy every other object
        newScene.MovedNetworkObjects = new NetworkObject[] { this.GetComponent<NetworkObject>() }; // keep this object in the next scene

        Debug.Log($"[SERVER] Launching game");

        // change scene
        InstanceFinder.SceneManager.LoadGlobalScenes(newScene);
    }

    public void ClickedStartGame()
    {
        Debug.Log("[CLIENT] Launching game");
        LaunchGame_Server();
    }

    private void OnSceneChangement(Scene scene, LoadSceneMode mode) {
        if (scene.name == "MainMenu") {
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

        if (scene.name == "LoadScene") {
            // if on server, reset the number of player ready when reloading a new game
            if (base.IsServerInitialized) {
                nbPlayerReady = 0;
            }
        }
    }






    /*
        ############################################################
                            Player list management
        ############################################################
    */

    private readonly Dictionary<NetworkConnection, PlayerData> serverPlayerList = new();
    private List<string> clientPlayerList = new();


    private IEnumerator SendPlayerName() {
        // fetch user data
        yield return authController.User();

        if (authController.CurrentUser == null) {
            Debug.LogError("[ERROR] Current user is null, cannot send data");
        } else {
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
    public void AddPlayerToList(NetworkConnection connection, string name) {
        Debug.Log($"[Lobby #number] New player connected : {name}");

        // add player to list
        serverPlayerList.Add(connection, new(name, maxPlayerLives, availableSkins[0]));
        OnPlayerListChange(PlayerData.GetNameList(serverPlayerList.Values.ToList()));

        // spawn player object
        playerSpawner.SpawnPlayer(connection, name);

        // Remove the skin given to new player from availables ones
        availableSkins.RemoveAt(0);
    }

    /// <summary>
    /// Add a player to the server-side playerList (call only from client-side)
    /// </summary>
    /// <param name="connection">Auto-fill by fishnet, the connection of the client who calls this procedure</param>
    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient(NetworkConnection connection = null) {
        if (connection == null) return;

        Debug.Log($"[Lobby #number] Disconnecting player : {serverPlayerList[connection].name}");

        // Break client connection
        connection.Disconnect(true);
    }

    /// <summary>
    /// Remove a player to the server-side playerList (call only from server-side)
    /// </summary>
    /// <param name="connection">The connection to remove</param>
    private void RemovePlayerFromList_ServerSide(NetworkConnection connection) {
        Debug.Log($"[Lobby #number] Removing player from list : {serverPlayerList[connection].name}");

        // despawn player
        PlayerData leavingPlayer = serverPlayerList[connection];
        playerSpawner.DespawnPlayer(leavingPlayer.name);

        // Add leaving player skins to available ones
        availableSkins.Add(leavingPlayer.skinColor);

        // remove player from list
        serverPlayerList.Remove(connection);
        OnPlayerListChange(PlayerData.GetNameList(serverPlayerList.Values.ToList()));
    }

    /// <summary>
    /// Function called by the server when the player list is actualized
    /// </summary>
    /// <param name="newNames">The new player list sent by the server</param>
    [ObserversRpc]
    private void OnPlayerListChange(List<string> newNames) {
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
    [Range(1, 6)]
    public int minimumPlayerToStart = 2;
    private InGameUiManager uiManager;
    private AuthController authController;






    /*
        ############################################################
                                Game Logic
        ############################################################
    */

    [Space(15)]
    [Header("Game parameters")]
    [Space(5)]

    [Range(1,5)]
    public int maxPlayerLives = 3;
    [Range(.5f, 5f)]
    public float respawnCooldown = 3f;
    public List<Material> skins = new();
    private readonly List<int> availableSkins = new();
    

    [ObserversRpc]
    private void SyncTexture(Dictionary<NetworkConnection, int> playersSkins) {

        // in game players
        if (CurrentSceneName() == "LoadScene") {
            Tank_Player[] playersInGame = FindObjectsByType<Tank_Player>(FindObjectsSortMode.None);
            foreach (Tank_Player player in playersInGame)
            {
                GameObject go = player.GameObject();
                Material skinColor = skins[playersSkins[go.GetComponent<NetworkObject>().Owner]];

                go.transform.Find("base").GetComponent<Renderer>().material = skinColor;
            }
        }

        // Lobby players
        Tank_Lobby[] playersLobby = FindObjectsByType<Tank_Lobby>(FindObjectsSortMode.None);
        foreach (Tank_Lobby player in playersLobby) {
            GameObject go = player.GameObject();
            Material skinColor = skins[playersSkins[go.GetComponent<NetworkObject>().Owner]];

            go.transform.Find("base").GetComponent<Renderer>().material = skinColor;
        }
    }

    public void OnPlayerHit(NetworkConnection connection) {
        try {
            Debug.Log($"Lose life : {serverPlayerList[connection].name}");
            serverPlayerList[connection].livesLeft--; // lose a life

            // despawn player
            playerSpawner.DespawnPlayer(serverPlayerList[connection].name);

            // if life left, respawn after a short duration
            if (serverPlayerList[connection].livesLeft > 0)
                StartCoroutine(RespawnCoroutine(connection, serverPlayerList[connection]));

        } catch {
            Debug.Log($"[ERROR] No player found for connection {connection}");
        }
    }

    private IEnumerator RespawnCoroutine(NetworkConnection playerConnection, PlayerData playerData) {
        
        // wait `respawnCooldown` seconds
        yield return new WaitForSeconds(respawnCooldown);

        // respawn player
        playerSpawner.SpawnPlayer(playerConnection, playerData.name);
        SyncTexture(PlayerData.GetSkinsDic(serverPlayerList));
    }
}

public class PlayerData
{
    public string name { get; set; }
    public int livesLeft { get; set; }
    public int skinColor { get; set; }

    public PlayerData(string _name, int _livesLeft, int _skinColor)
    {
        name = _name;
        livesLeft = _livesLeft;
        skinColor = _skinColor;
    }

    static public List<string> GetNameList(List<PlayerData> playerDatas) {
        List<string> names = new();

        foreach(PlayerData player in playerDatas) {
            names.Add(player.name);
        }

        return names;
    }

    static public Dictionary<NetworkConnection, int> GetSkinsDic(Dictionary<NetworkConnection, PlayerData> players) {
        Dictionary<NetworkConnection, int> skins = new();

        foreach (KeyValuePair<NetworkConnection, PlayerData> pair in players) {
            skins.Add(pair.Key, pair.Value.skinColor);
        }

        return skins;
    }
}