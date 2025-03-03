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

                transport.SetClientAddress(serverIP);
                transport.SetPort((ushort)serverPort); 

                Debug.Log($"[CLIENT] Connecting to {serverIP}:{serverPort}...");
                nm.ClientManager.StartConnection();
            }
            else
            {
                Debug.LogError("Tugboat transport not found on the NetworkManager.");
            }
        }
    }

    private string GetServerIP()
    {
        string ip = Environment.GetEnvironmentVariable("GAME_SERVER_IP");
        return string.IsNullOrEmpty(ip) ? "127.0.0.1" : ip; 
    }

    private int GetServerPort()
    {
        string port = Environment.GetEnvironmentVariable("GAME_SERVER_PORT");
        return int.TryParse(port, out int result) ? result : 7777; // Default port
    }
}