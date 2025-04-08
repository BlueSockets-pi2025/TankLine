using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class WaitingRoomMenu : NetworkBehaviour 
{
    private List<string> playerList = new();
    public GameObject playerListDiv;
    public GameObject playerEntryPrefab;
    private AuthController authController;

    /// <summary>
    /// Add a player to the server-side playerList
    /// </summary>
    /// <param name="name">The username to add</param>
    [ServerRpc(RequireOwnership = false)]
    private void AddPlayerToList(string name) {
        Debug.Log($"[Waiting-Room] New player connected : {name}");
        playerList.Add(name);
        OnPlayerListChange(playerList);
    }

    /// <summary>
    /// Remove a player to the server-side playerList
    /// </summary>
    /// <param name="name">The username to remove</param>
    [ServerRpc(RequireOwnership = false)]
    private void RemovePlayerFromList(string name) {
        Debug.Log($"[Waiting-Room] Player disconnected : {name}");
        playerList.Remove(name);
        OnPlayerListChange(playerList);
    }

    /// <summary>
    /// Function called by the server when the player list is actualized
    /// </summary>
    /// <param name="newList">The new player list sent by the server</param>
    [ObserversRpc]
    private void OnPlayerListChange(List<string> newList) {
        Debug.Log($"New list : {newList}");

        // if a new player connect, put his name
        if (playerList.Count < newList.Count) {
            for (int i=playerList.Count; i<newList.Count; i++) {
                GameObject newEntry = Instantiate(playerEntryPrefab, playerListDiv.transform);
                newEntry.name = newList[i];
                newEntry.GetComponent<TMP_Text>().text = newList[i];
            }
        }

        // if a player leave, find then remove his name
        else {
            string removed;
            foreach (string name in playerList) {
                if (!newList.Exists(x => x==name))
                    removed = name;
            }

            Destroy(playerListDiv.transform.Find(name).gameObject);
        }

        playerList = newList;
    }

    void Start() {
        if (base.IsClientInitialized) {
            authController = gameObject.GetComponent<AuthController>();
            
            StartCoroutine(SendPlayerName());
        }
    }

    private IEnumerator SendPlayerName() {
        // fetch user data
        yield return authController.User();

        if (authController.CurrentUser == null) {
            Debug.LogError("[ERROR] Current user is null");
        } else {
            // send username to server
            string userName = authController.CurrentUser.username.ToString();
            AddPlayerToList(userName);
        }
    }

    private void OnDestroy() {
        if (authController.CurrentUser != null)
            Debug.Log("Remove player");
            RemovePlayerFromList(authController.CurrentUser.username.ToString());
    }
}