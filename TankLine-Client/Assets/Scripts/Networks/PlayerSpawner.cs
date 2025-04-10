using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Spawner objects references")]
    [Space(10)]
    public NetworkObject playerObjectPrefab = null;
    public List<GameObject> spawnPoints = new();

    private int currentSpawnIndex = 0;

    void Awake()
    {
        if (playerObjectPrefab == null) {
            Debug.LogError("[ERROR] Player object prefab not referenced");
            this.enabled = false;
        } else if (spawnPoints.Count == 0) {
            Debug.LogError("[ERROR] No spawn point referenced");
            this.enabled = false;
        }
    }

    /// <summary>
    /// Spawn a player on one of the spawnPoint
    /// </summary>
    /// <param name="ownerConnection">The connection of the player who will own the object</param>
    public void SpawnPlayer(NetworkConnection ownerConnection, string ownerName) {

        // instantiate the new player at the current spawnpoint position
        GameObject newPlayer = Instantiate(playerObjectPrefab.gameObject);
        newPlayer.transform.position = spawnPoints[currentSpawnIndex].transform.position;
        newPlayer.name = "Player:" + ownerName;

        // spawn the player
        Spawn(newPlayer, ownerConnection);

        // increment current spawn point
        currentSpawnIndex++;
        if (currentSpawnIndex >= spawnPoints.Count) currentSpawnIndex -= spawnPoints.Count;
    }

    /// <summary>
    /// Despawn a player
    /// </summary>
    /// <param name="playerObject">The reference to the player gameObject</param>
    public void DespawnPlayer(GameObject playerObject) {
        Despawn(playerObject);
    }

    /// <summary>
    /// Despawn a player
    /// </summary>
    /// <param name="playerName">The name of the player</param>
    public void DespawnPlayer(string playerName) {
        GameObject playerObject = GameObject.Find("Player:"+playerName);
        Despawn(playerObject);
    }
}
