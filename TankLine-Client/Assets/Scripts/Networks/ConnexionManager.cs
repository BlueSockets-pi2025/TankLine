using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;
using System.IO;
using System.Collections.Generic;

public class ConnexionManager : MonoBehaviour
{
    private NetworkManager networkManager;
    private Tugboat tugboat;
    public const string PathToEnvFile = "/.env";
    public EnvVariables serverConfig;


    private void Awake() {
        // initialize variables
        networkManager = FindFirstObjectByType<NetworkManager>();
        tugboat = networkManager.GetComponent<Tugboat>();

        // check if the script is running on the server with the env variable "IS_DEDICATED_SERVER"
        string isDedicatedServer = Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER");

        if (String.Compare(isDedicatedServer, "true") == 0) {

            if (networkManager && tugboat) {

                // open server port in the VM
                int serverPort = 10001;
                tugboat.SetPort((ushort)serverPort);

                // start the server connection
                networkManager.ServerManager.StartConnection();

                // add a log for when client connects
                networkManager.ServerManager.OnRemoteConnectionState += (conn, state) => {
                    //Debug.Log($"[SERVER] Client {conn.ClientId} State: {state}");
                };

                Debug.Log("[SERVER] Server initialized");
            } else {
                Debug.LogError("[SERVER] Error: networkManager or tugboat not found");
            }

        }

        // if the script is not running on dedicated server, connect as a client
        else {
            if (networkManager && tugboat) {
                LoadServerConfig();

                // set the server IP and PORT for client connexion
                tugboat.SetClientAddress(string.IsNullOrEmpty(serverConfig.GAME_SERVER_IP) ? "127.0.0.0" : serverConfig.GAME_SERVER_IP);
                tugboat.SetPort((ushort)(int.TryParse(serverConfig.GAME_SERVER_PORT, out int result) ? result : 7770));

                networkManager.ClientManager.StartConnection();
            } else {
                Debug.LogError("[SERVER] Error: networkManager or tugboat not found");
            }
        }
    }

    /// <summary>
    /// Load the server configuration from the `.env` file and store it in the variable `serverConfig`
    /// </summary>
    private void LoadServerConfig()
    {
        string jsonEnv = File.ReadAllText(Application.streamingAssetsPath + PathToEnvFile);
        serverConfig = JsonUtility.FromJson<EnvVariables>(jsonEnv);
    }
}
