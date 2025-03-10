using UnityEngine;
using FishNet;

public class ConnectionManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #if UNITY_EDITOR
        InstanceFinder.ServerManager.StartConnection();
        #else
        InstanceFinder.ClientManager.StartConnection();
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown) {
            InstanceFinder.ClientManager.StartConnection();
        }
    }
}
