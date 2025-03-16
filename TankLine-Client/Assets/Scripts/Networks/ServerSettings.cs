using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;

public class ServerSettings : MonoBehaviour
{
  private NetworkManager nm;
  private void Awake()
  {
    // check if the script is running on the server with the env variable "IS_DEDICATED_SERVER"
    string isDedicatedServer = Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER");

    if (String.Compare(isDedicatedServer, "true") == 0) {
      Debug.Log("[SERVER] Server initialized");

      nm = FindFirstObjectByType<NetworkManager>();
      if (nm)
      {
        Tugboat transport = nm.GetComponent<Tugboat>();

        int serverPort = 10001; // set the server port open in the VM

        transport.SetPort((ushort)serverPort);

        nm.ServerManager.StartConnection(); // start the server connection
        nm.ServerManager.OnRemoteConnectionState += (conn, state) =>
        {
          Debug.Log($"[SERVER] Client {conn.ClientId} State: {state}");
        };
      }
    }
  }
}