using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;

public class ConnectionManagerTests
{
    private GameObject connectionManagerGO;
    private ConnectionManager connectionManager;
    private GameObject networkManagerPrefab;

    [SetUp]
    public void SetUp()
    {
        connectionManagerGO = new GameObject("ConnectionManager");
        connectionManager = connectionManagerGO.AddComponent<ConnectionManager>();

        networkManagerPrefab = Resources.Load<GameObject>("Prefabs/NetworkManager");
        Assert.IsNotNull(networkManagerPrefab, "[TEST] NetworkManager prefab not found.");

        GameObject networkManagerInstance = GameObject.Instantiate(networkManagerPrefab);
        networkManagerInstance networkManager = networkManagerInstance.GetComponent<NetworkManager>();

        connectionManager.GetType().GetField("networkManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(connectionManager, networkManager);
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(connectionManagerGO);
    }

    [UnityTest]
    public IEnumerator TestServerInitialization()
    {
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "true");
        connectionManager.Awake();

        yield return new WaitForSeconds(1);

        Assert.IsTrue(connexionManager.GetComponent<NetworkManager>().ServerManager.Started, "[SERVER] Server should be running.");
    }

    [UnityTest]
    public IEnumerator TestClientConnection()
    {
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "false");
        connectionManager.Awake();

        yield return new WaitForSeconds(1);

        Assert.IsTrue(connexionManager.GetComponent<NetworkManager>().ClientManager.Started, "[CLIENT] Client should be connected.");
    }

    [UnityTest]
    public IEnumerator TestMultipleClientsConnection()
    {
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "true");
        networkManager.ServerManager.StartConnection();
        yield return new WaitForSeconds(1);
        Assert.IsTrue(networkManager.ServerManager.Started, "[SERVER] Server should be running.");

        // Simulate first client connecting
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "false");
        networkManager.ClientManager.StartConnection();
        yield return new WaitForSeconds(1);
        Assert.IsTrue(networkManager.ClientManager.Started, "[CLIENT] First client should be connected.");

        // Simulate second client connecting
        GameObject secondClientObject = new GameObject("SecondClient");
        NetworkManager secondClientManager = secondClientObject.AddComponent<NetworkManager>();
        secondClientManager.ClientManager.StartConnection();
        yield return new WaitForSeconds(1);

        Assert.IsTrue(secondClientManager.ClientManager.Started, "[CLIENT] Second client should be connected.");
    }
}