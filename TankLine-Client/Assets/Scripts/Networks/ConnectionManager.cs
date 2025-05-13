using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;
using System.IO;
using System.Collections;   
using UnityEngine.Networking;

public class ConnectionManager : MonoBehaviour
{
    private static ConnectionManager instance = null;
    private NetworkManager networkManager;
    private Tugboat tugboat;
    public const string envFile = "/.env";
    public EnvVariables serverConfig;

    private void Awake() {

        if (instance != null) {
            gameObject.GetComponent<ConnectionManager>().enabled = false;
            return;
        }
        instance = this;

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
                StartCoroutine(StartClient());
            } else {
                Debug.LogError("[SERVER] Error: networkManager or tugboat not found");
            }
        }
    }

    /// <summary>
    /// Start the client connection
    /// </summary>
    private IEnumerator StartClient()
    {
        // Load the server configuration
        yield return LoadServerConfig(); // TO be sure the serverConfig is loaded before starting the client

        if (serverConfig == null)
        {
            Debug.LogError("Server configuration is null. Cannot proceed with client initialization.");
            yield break;
        }

        // Set the server IP and PORT for client connexion
        tugboat.SetClientAddress(string.IsNullOrEmpty(serverConfig.GAME_SERVER_IP) ? "127.0.0.0" : serverConfig.GAME_SERVER_IP);
        tugboat.SetPort((ushort)(int.TryParse(serverConfig.GAME_SERVER_PORT, out int result) ? result : 7770));

        // Start the client connection
        networkManager.ClientManager.StartConnection();
    }

    /// <summary>
    /// Load the server configuration from the `.env` file and store it in the variable `serverConfig`
    /// </summary>
    private IEnumerator LoadServerConfig()
    {
        string envFilePath;

        #if UNITY_ANDROID || UNITY_IOS
            Debug.Log("Running on Android. Attempting to load .env file using UnityWebRequest.");
            envFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, ".env");
            yield return StartCoroutine(LoadServerConfigForAndroid(envFilePath));
        #else 
            envFilePath = Application.streamingAssetsPath + "./env";
            if (File.Exists(envFilePath))
            {
                string jsonEnv = File.ReadAllText(envFilePath);
                serverConfig = JsonUtility.FromJson<EnvVariables>(jsonEnv);
                Debug.Log("Server configuration successfully loaded.");
            }
            else
            {
                Debug.LogError("Env file not found! Ensure it's in the correct directory.");
            }
            yield return null;
        #endif
    }

    private IEnumerator LoadServerConfigForAndroid(string envFilePath)
    {
        UnityWebRequest request = UnityWebRequest.Get(envFilePath);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            string envContent = request.downloadHandler.text;
            serverConfig = JsonUtility.FromJson<EnvVariables>(envContent);

            Debug.Log("Server configuration successfully loaded on Android.");
            Debug.Log("Env return: " + envContent);
            Debug.Log("ServerConfig: " + JsonUtility.ToJson(serverConfig));

        }
        else
        {
            Debug.LogError("Failed to load .env file on Android: " + request.error);
        }
    }
}


