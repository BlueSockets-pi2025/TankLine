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

        Assert.IsTrue(networkManager.ClientManager.Started, "[CLIENT] Client should be connected.");
    }

    [UnityTest]
    public IEnumerator TestMultipleClients()
    {
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "true");
        connectionManager.Awake();

        yield return new WaitForSeconds(1);

        // Simulate two clients connecting
        var client1 = new GameObject().AddComponent<ConnectionManager>();
        var client2 = new GameObject().AddComponent<ConnectionManager>();

        client1.Awake();
        client2.Awake();

        yield return new WaitForSeconds(2);

        Assert.IsTrue(networkManager.ServerManager.Clients.Count == 2, "[SERVER] Should have two connected clients.");
    }
}