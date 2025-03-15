using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;

public class ClientSettings : MonoBehaviour
{
    private NetworkManager nm;
    private void Start()
    {
        nm = FindObjectOfType<NetworkManager>();
        if (nm)
        {
            Tugboat transport = nm.GetComponent<Tugboat>();

            if (transport != null)
            {
                string serverIP = GetServerIP();
                int serverPort = GetServerPort();

                transport.SetClientAddress(serverIP);
                transport.SetPort((ushort)serverPort);

                nm.ClientManager.StartConnection();
            }
        }
    }

    private string GetServerIP()
    {
        string ip = Environment.GetEnvironmentVariable("GAME_SERVER_IP");
        return string.IsNullOrEmpty(ip) ? "127.0.0.0" : ip; // localhost if no env variable
    }

    private int GetServerPort()
    {
        string port = Environment.GetEnvironmentVariable("GAME_SERVER_PORT");
        return int.TryParse(port, out int result) ? result : 7770; // Default port if no env variable
    }
}