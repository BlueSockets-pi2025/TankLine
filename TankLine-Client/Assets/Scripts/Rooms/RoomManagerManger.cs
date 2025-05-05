using UnityEngine;
using FishNet.Object;
using System;

public class RoomManagerManger : NetworkBehaviour
{
    public GameObject roomManagerPrefab;

    void Start()
    {
        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") != "true")
        {
            Destroy(this.gameObject);
            return;
        }

        if (FindFirstObjectByType<RoomManager>() != null)
        {
            Debug.Log("[WARNING] RoomManager already exists. Destroying duplicate.");
            Destroy(this.gameObject);
            return;
        }

        Debug.Log("[SERVER] Spawning RoomManager.");

        GameObject newRoomManager = Instantiate(roomManagerPrefab);
        newRoomManager.GetComponent<NetworkObject>().SetIsGlobal(true);
        Spawn(newRoomManager, null);

        Destroy(this.gameObject);
    }
}
