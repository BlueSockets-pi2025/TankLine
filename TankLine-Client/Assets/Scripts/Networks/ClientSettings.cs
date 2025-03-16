using UnityEngine;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using System;
using System.IO;

public class ClientSettings : MonoBehaviour
{
    public const string PathToEnvFile = "/Scripts/.env";
    public EnvVariables serverConfig;
    private NetworkManager nm;
    private void Start()
    {
        nm = FindObjectOfType<NetworkManager>();
        if (nm)
        {
            Tugboat transport = nm.GetComponent<Tugboat>();

            if (transport != null)
            {
                LoadServerConfig();
                Debug.Log(serverConfig);

                transport.SetClientAddress(string.IsNullOrEmpty(serverConfig.GAME_SERVER_IP) ? "127.0.0.0" : serverConfig.GAME_SERVER_IP);
                transport.SetPort((ushort) (int.TryParse(serverConfig.GAME_SERVER_PORT, out int result) ? result : 7770));

                nm.ClientManager.StartConnection();
            }
        }
    }

    private void LoadServerConfig()
    {
        string jsonEnv = File.ReadAllText(Application.dataPath + PathToEnvFile);
        serverConfig = JsonUtility.FromJson<EnvVariables>(jsonEnv);
    }
}