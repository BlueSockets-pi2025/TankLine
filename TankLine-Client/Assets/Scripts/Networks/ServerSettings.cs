using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;

public class ServerSettings : MonoBehaviour
{
  private void Start()
  {
    if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
    {
      NetworkManager nm = FindObjectOfType<NetworkManager>();
      if (nm)
      {
        Tugboat transport = nm.GetComponent<Tugboat>();

        // Ensure transport is enabled
        if (transport != null)
        {
          int serverPort = GetServerPort();

          // Tugboat does NOT have SetServerPort(), so we set it in the inspector or manually in Unity.
          Debug.Log($"[SERVER] Starting Tugboat on port {serverPort}...");

          nm.ServerManager.StartConnection();
        }
      }
    }
  }

  private int GetServerPort()
  {
    string port = Environment.GetEnvironmentVariable("GAME_SERVER_PORT");
    return int.TryParse(port, out int result) ? result : 7777; // Default port
  }
}