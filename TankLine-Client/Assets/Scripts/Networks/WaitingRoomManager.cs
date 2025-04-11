using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Observing;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AuthController))]
[RequireComponent(typeof(NetworkObserver))]
[RequireComponent(typeof(PlayerSpawner))]
public class WaitingRoomManager : NetworkBehaviour 
{
    private readonly Dictionary<NetworkConnection, string> serverPlayerList = new();

    private List<string> clientPlayerList = new();
    private GameObject playerListDiv;
    private GameObject playerCount;
    private bool isSettingsPanelOpen = false;
    private AuthController authController;
    private PlayerSpawner playerSpawner;

    [Header("UI References")]
    [Space(5)]
    public GameObject canvas;
    public GameObject playerEntryPrefab;

    void Awake() {
        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") != "true") return;

        // find networkManager
        NetworkManager networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager == null) {
            Debug.LogError($"[ERROR] NetworkManager is null in waiting room");
            return;
        }

        // on client connexion change
        networkManager.ServerManager.OnRemoteConnectionState += (connexion, state) => {
            // if client is disconnectingPlayerSpconnection
            if (state.ConnectionState == FishNet.Transporting.RemoteConnectionState.Stopped) {
                // remove from list
                RemovePlayerFromList_ServerSide(connexion);
            }
        };
    }

    /// <summary>
    /// Add a player to the server-side playerList
    /// </summary>
    /// <param name="name">The username to add</param>
    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerToList(NetworkConnection connection, string name) {
        Debug.Log($"[Waiting-Room] New player connected : {name}");

        // add player to list
        serverPlayerList.Add(connection, name);
        OnPlayerListChange(serverPlayerList.Values.ToList());

        // spawn player object
        playerSpawner.SpawnPlayer(connection, name);
    }

    /// <summary>
    /// Add a player to the server-side playerList (call only from client-side)
    /// </summary>
    /// <param name="connection">Auto-fill by fishnet, the connection of the client who calls this procedure</param>
    [ServerRpc(RequireOwnership = false)]
    public void DisconnectClient(NetworkConnection connection = null) {
        if (connection == null) return;

        Debug.Log($"[Waiting-Room] Disconnecting player : {serverPlayerList[connection]}");

        // despawn player
        string playerName = serverPlayerList[connection];
        playerSpawner.DespawnPlayer(playerName);

        // remove player from list
        serverPlayerList.Remove(connection);
        OnPlayerListChange(serverPlayerList.Values.ToList());

        // Break client connection
        connection.Disconnect(false);
    }

    /// <summary>
    /// Remove a player to the server-side playerList (call only from server-side)
    /// </summary>
    /// <param name="connection">The connection to remove</param>
    private void RemovePlayerFromList_ServerSide(NetworkConnection connection) {
        Debug.Log($"[Waiting-Room] Disconnecting player : {serverPlayerList[connection]}");

        // despawn player
        string playerName = serverPlayerList[connection];
        playerSpawner.DespawnPlayer(playerName);

        // remove player from list
        serverPlayerList.Remove(connection);
        OnPlayerListChange(serverPlayerList.Values.ToList());
    }

    /// <summary>
    /// Function called by the server when the player list is actualized
    /// </summary>
    /// <param name="newNames">The new player list sent by the server</param>
    [ObserversRpc]
    private void OnPlayerListChange(List<string> newNames) {
        clientPlayerList = newNames;

        // clear the name list
        foreach (Transform child in playerListDiv.transform) {
            Destroy(child.gameObject);
        }

        // put every entry in the UI list
        foreach (string name in clientPlayerList) {
            GameObject newEntry = Instantiate(playerEntryPrefab, playerListDiv.transform);
            newEntry.name = name;
            newEntry.GetComponent<TMP_Text>().text = name;
        }

        // change player count on UI
        playerCount.GetComponent<TMP_Text>().text = clientPlayerList.Count.ToString() + "/6";
    }

    void Start() {
        playerListDiv = canvas.transform.Find("PlayersCanvas").gameObject;
        playerCount = canvas.transform.Find("PlayerCount").gameObject;
        playerSpawner = gameObject.GetComponent<PlayerSpawner>();


        // send username to DB
        if (base.IsClientInitialized) {
            authController = gameObject.GetComponent<AuthController>();
            StartCoroutine(SendPlayerName());
        }
    }

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

    public void ExitToMenu() {
        DisconnectClient();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ClickPanel() {
        isSettingsPanelOpen = !isSettingsPanelOpen;
        canvas.transform.Find("SettingsPanel").gameObject.SetActive(isSettingsPanelOpen);
    }
}