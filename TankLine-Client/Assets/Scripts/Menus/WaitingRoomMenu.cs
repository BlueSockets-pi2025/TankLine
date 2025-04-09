using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WaitingRoomMenu : NetworkBehaviour 
{
    private readonly Dictionary<NetworkConnection, string> serverPlayerList = new();

    private List<string> clientPlayerList = new();
    public GameObject canvas;
    private GameObject playerListDiv;
    private GameObject playerCount;
    public GameObject playerEntryPrefab;
    private AuthController authController;
    private bool isSettingsPanelOpen = false;

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

    /// <summary>
    /// Add a player to the server-side playerList
    /// </summary>
    /// <param name="name">The username to add</param>
    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerToList(NetworkConnection connection, string name) {
        Debug.Log($"[Waiting-Room] New player connected : {name}");
        serverPlayerList.Add(connection, name);
        OnPlayerListChange(serverPlayerList.Values.ToList());
    }

    /// <summary>
    /// Remove a player to the server-side playerList
    /// </summary>
    /// <param name="name">The username to remove</param>
    private void RemovePlayerFromList_ServerSide(NetworkConnection connexion) {
        Debug.Log($"[Waiting-Room] Disconnecting player : {serverPlayerList[connexion]}");
        serverPlayerList.Remove(connexion);
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
        if (canvas == null) {
            canvas = gameObject;
        }

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
            AddPlayerToList(base.ClientManager.Connection, userName);
        }
    }

    public void ExitToMenu() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ClickPanel() {
        isSettingsPanelOpen = !isSettingsPanelOpen;
        canvas.transform.Find("SettingsPanel").gameObject.SetActive(isSettingsPanelOpen);
    }
}