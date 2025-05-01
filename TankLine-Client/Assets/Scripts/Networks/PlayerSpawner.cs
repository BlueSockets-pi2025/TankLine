using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [Header("Prefabs references")]
    [Space(10)]
    public GameObject playerObjectPrefab = null;
    private List<GameObject> spawnPoints = new();

    private int currentSpawnIndex = 0;

    public void InitSpawnPoint()
    {
        // find new spawnpoint transform
        GameObject spawnPointsParent = GameObject.Find("PlayerSpawns");
        if (spawnPointsParent == null) {
            Debug.Log("[ERROR] spawnpoint parent not found");
            return;
        }

        // clear old spawnpoints
        spawnPoints = new();

        foreach (Transform child in spawnPointsParent.transform) {
            spawnPoints.Add(child.gameObject);
        }

        Debug.Log($"{spawnPoints.Count} new spawn point initialized");
    }

    /// <summary>
    /// Spawn a player on one of the spawnPoint
    /// </summary>
    /// <param name="ownerConnection">The connection of the player who will own the object</param>
    /// <param name="ownerName">The owner nickname</param>
    public void SpawnPlayer(NetworkConnection ownerConnection, string ownerName) {

        // instantiate the new player at the current spawnpoint position
        GameObject newPlayer = Instantiate(playerObjectPrefab);
        newPlayer.transform.position = spawnPoints[currentSpawnIndex].transform.position;

        // spawn the player
        Spawn(newPlayer, ownerConnection);
        newPlayer.name = "Player:" + ownerName;

        // increment current spawn point
        currentSpawnIndex++;
        if (currentSpawnIndex >= spawnPoints.Count) currentSpawnIndex -= spawnPoints.Count;
    }

    /// <summary>
    /// Spawn a player and set his skin
    /// </summary>
    /// <param name="ownerConnection">The connection of the player who will own the object</param>
    /// <param name="ownerName">The owner nickname</param>
    /// <param name="skinColor">The skin color</param>
    public void SpawnPlayerWithSkin(NetworkConnection ownerConnection, string ownerName, Material skinColor) {

        // instantiate the new player at the current spawnpoint position
        GameObject newPlayer = Instantiate(playerObjectPrefab);
        newPlayer.transform.position = spawnPoints[currentSpawnIndex].transform.position;

        // spawn the player
        Spawn(newPlayer, ownerConnection);
        newPlayer.name = "Player:" + ownerName;

        // apply the skin
        try {
            newPlayer.transform.Find("base").GetComponent<Renderer>().material = skinColor;

            foreach (Transform tr in newPlayer.transform.Find("tankGun"))
                tr.GetComponent<Renderer>().material = skinColor;
        } catch (NullReferenceException) {
            Debug.Log("[ERROR] Renderer not found");
        }

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

    /// <summary>
    /// Despawn every player
    /// </summary>
    public void DespawnEveryone(List<string> playersName) {
        foreach (string player in playersName) {
            GameObject playerObject = GameObject.Find("Player:"+player);
            Despawn(playerObject);
        }
    }
}
