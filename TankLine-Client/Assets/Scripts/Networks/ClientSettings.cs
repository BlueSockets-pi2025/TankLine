using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;

public class ClientSettings : MonoBehaviour
{
  private void Start()
  {
    NetworkManager nm = FindObjectOfType<NetworkManager>();
    if (nm)
    {
      Tugboat transport = nm.GetComponent<Tugboat>();

      if (transport != null)
      {
        string serverIP = GetServerIP();
        int serverPort = GetServerPort();

        // Tugboat does not use direct methods like SetClientAddress(), so we modify the settings dynamically.
        transport.SetClientAddress(serverIP);
        transport.SetPort((ushort)serverPort); // Make sure the port is cast to ushort

        Debug.Log($"[CLIENT] Connecting to {serverIP}:{serverPort}...");
        nm.ClientManager.StartConnection();
      }
    }
  }

  private string GetServerIP()
  {
    string ip = Environment.GetEnvironmentVariable("GAME_SERVER_IP");
    return string.IsNullOrEmpty(ip) ? "127.0.0.1" : ip; // Default to localhost
  }

  private int GetServerPort()
  {
    string port = Environment.GetEnvironmentVariable("GAME_SERVER_PORT");
    return int.TryParse(port, out int result) ? result : 7777; // Default port
  }
}