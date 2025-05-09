using System;
using FishNet.Object;
using UnityEngine;

public class LobbyManagerManager : NetworkBehaviour
{
    public GameObject LobbyManagerPrefab;

    void Start()
    {
        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") != "true") {
            Destroy(this.gameObject);
            return; 
        }

        if (FindFirstObjectByType<LobbyManager>() != null) {
            Debug.Log("[WARNING] LobbyManager already exist, destroying this gameobject");
            Destroy(this.gameObject);
            return;
        }
        
        Debug.Log("[SERVER] Spawning LobbyManager");

        // spawn lobbyManager as dontDestroyOnLoad
        GameObject newLobbyManager = Instantiate(LobbyManagerPrefab);
        newLobbyManager.GetComponent<NetworkObject>().SetIsGlobal(true);
        Spawn(newLobbyManager, null);

        // destroy this object
        Destroy(this.gameObject);
    }
}
