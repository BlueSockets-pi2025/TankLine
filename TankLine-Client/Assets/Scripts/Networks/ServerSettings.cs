using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;

public class ServerSettings : MonoBehaviour
{
  private void Awake()
  {
    if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) // verify if the server is headless
    {
      NetworkManager nm = FindObjectOfType<NetworkManager>();
      if (nm)
      {
        Tugboat transport = nm.GetComponent<Tugboat>(); 

        int serverPort = GetServerPort();

        transport.SetPort((ushort)serverPort); 

        nm.ServerManager.StartConnection(); // start the server connection
      }
    }
  }

  //get the server port from the environment variable
  private int GetServerPort()
  {
    string port = Environment.GetEnvironmentVariable("GAME_SERVER_PORT");
    return int.TryParse(port, out int result) ? result : 7770; // Default port
  }
}