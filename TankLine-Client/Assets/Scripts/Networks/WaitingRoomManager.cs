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
using UnityEngine.UI;

[RequireComponent(typeof(AuthController))]
[RequireComponent(typeof(NetworkObserver))]
[RequireComponent(typeof(PlayerSpawner))]
public class WaitingRoomManager : NetworkBehaviour 
{
    private readonly Dictionary<NetworkConnection, string> serverPlayerList = new();

    private List<string> clientPlayerList = new();
    private PlayerSpawner playerSpawner;
    private AuthController authController;

    private WaitingRoomUiManager uiManager;

    [Header("UI References")]
    [Space(5)]
    
    public GameObject playerEntryPrefab;
    [Range(1,6)]
    public int minimumPlayerToStart = 2;

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
            // if client is disconnecting
            if (state.ConnectionState == FishNet.Transporting.RemoteConnectionState.Stopped) {
                // remove from list
                RemovePlayerFromList_ServerSide(connexion);
            }
        };
    }

    void Start() {
        playerSpawner = gameObject.GetComponent<PlayerSpawner>();
        uiManager = new(GameObject.Find("Canvas"));

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

        uiManager.UpdateUI(clientPlayerList, playerEntryPrefab, minimumPlayerToStart);
    }

    [ServerRpc(RequireOwnership=false)]
    public void LaunchGame_Server() {
        // launch game on every client
        LaunchGame_Client();

        // change scene on the server
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGameScene");
    }

    [ObserversRpc]
    private void LaunchGame_Client() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("InGameScene");
    }


    /*
        ############################################################
                          UI Manager (waiting room)
        ############################################################
    */

    private class WaitingRoomUiManager 
    {
        private GameObject canvas;
        private GameObject playerListDiv;
        private GameObject playerCount;
        private GameObject startButton;
        private bool isSettingsPanelOpen = false;

        public WaitingRoomUiManager(GameObject _canvas) {
            canvas = _canvas;
            if (canvas == null) {
                Debug.LogError("Critical error, canvas is null");
                return;
            }

            // search gameobject
            playerListDiv = canvas.transform.Find("PlayersCanvas").gameObject;
            playerCount = canvas.transform.Find("PlayerCount").gameObject;
            startButton = canvas.transform.Find("StartButton").gameObject;

            // disable start button at the beggining
            DisableStartButton();

            // add onclick function to buttons
            canvas.transform.Find("GearButton").GetComponent<Button>().onClick.AddListener(this.ClickPanel);
            canvas.transform.Find("SettingsPanel").Find("ExitButton").GetComponent<Button>().onClick.AddListener(this.ExitToMenu);
        }


        public void ClickPanel() {
            isSettingsPanelOpen = !isSettingsPanelOpen;
            canvas.transform.Find("SettingsPanel").gameObject.SetActive(isSettingsPanelOpen);
        }

        public void ExitToMenu() {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        private void DisableStartButton() {
            startButton.GetComponent<Button>().interactable = false;
            startButton.transform.Find("startText").GetComponent<TMP_Text>().color = new(.5f, .5f, .5f, .5f);
        }

        private void EnableStartButton() {
            startButton.GetComponent<Button>().interactable = true;
            startButton.transform.Find("startText").GetComponent<TMP_Text>().color = new(0f, 0f, 0f, 1f);
        }

        /// <summary>
        /// Update the whole UI of the waiting room (player names, player count & start button)
        /// </summary>
        /// <param name="clientPlayerList">The list of the player names</param>
        /// <param name="playerEntryPrefab">The prefab of a single player name entry</param>
        /// <param name="minimumPlayerToStart">The minimum number of player required to be able to start a game</param>
        public void UpdateUI(List<string> clientPlayerList, GameObject playerEntryPrefab, int minimumPlayerToStart) {
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

            // enable/disable start button if enough people
            if (clientPlayerList.Count >= minimumPlayerToStart)
                EnableStartButton();
            else
                DisableStartButton();
        }
    }
}