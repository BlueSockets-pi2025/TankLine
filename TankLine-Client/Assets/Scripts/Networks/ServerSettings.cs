using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;

public class ServerSettings : MonoBehaviour
{
  private NetworkManager nm;
  private void Awake()
  {
    if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) // verify if the server is headless
    {
      nm = FindObjectOfType<NetworkManager>();
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