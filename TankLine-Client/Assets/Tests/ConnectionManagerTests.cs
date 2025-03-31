using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using FishNet.Managing;
using FishNet.Transporting.Tugboat;
using UnityEditor;

public class ConnectionManagerTests
{
    private GameObject serverGO;
    private GameObject client1GO;
    private GameObject client2GO;
    private GameObject networkManagerPrefab;
    [SetUp]
    public void SetUp()
    {
        #if UNITY_EDITOR
        networkManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NetworkManager.prefab");
        Assert.IsNotNull(networkManagerPrefab, "[TEST] NetworkManager prefab not found.");
        #else
            Assert.Fail("Tests require Unity Editor to run.");
        #endif
    }

    [TearDown]
    public void TearDown()
    {
        if (serverGO) GameObject.DestroyImmediate(serverGO);
        if (client1GO) GameObject.DestroyImmediate(client1GO);
        if (client2GO) GameObject.DestroyImmediate(client2GO);
    }

    /// <summary>
    /// Test if server is initialized correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator TestServerInit()
    {
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "true");

        serverGO = Object.Instantiate(networkManagerPrefab);
        yield return new WaitForSeconds(1);

        var serverNM = serverGO.GetComponent<NetworkManager>();
        Assert.IsTrue(serverNM.ServerManager.Started, "[SERVER] Server should be initialized.");
    }

    /// <summary>
    /// Test if client is connected.
    /// </summary>
    [UnityTest]
    public IEnumerator TestSingleClientConn()
    {
        // Start server
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "true");

        serverGO = Object.Instantiate(networkManagerPrefab);
        yield return new WaitForSeconds(1);

        var serverNM = serverGO.GetComponent<NetworkManager>();
        Assert.IsTrue(serverNM.ServerManager.Started, "[SERVER] Server should be running.");

        // Start client
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "false");
        client1GO = Object.Instantiate(networkManagerPrefab);
        yield return new WaitForSeconds(2);

        var client1NM = client1GO.GetComponent<NetworkManager>();
        Assert.IsTrue(client1NM.ClientManager.Connected, "[CLIENT] Client should be connected.");
    }

    /// <summary>
    /// Test if multiple clients are connected.
    /// </summary>
    [UnityTest]
    public IEnumerator TestMultipleClientsConn()
    {
        // Start server
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "true");

        serverGO = Object.Instantiate(networkManagerPrefab);
        yield return new WaitForSeconds(1);

        var serverNM = serverGO.GetComponent<NetworkManager>();
        Assert.IsTrue(serverNM.ServerManager.Started, "[SERVER] Server should be running.");

        // Start client 1
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "false");
        client1GO = Object.Instantiate(networkManagerPrefab);
        yield return new WaitForSeconds(2);

        var client1NM = client1GO.GetComponent<NetworkManager>();
        Assert.IsTrue(client1NM.ClientManager.Connected, "[CLIENT 1] Client should be connected.");

        // Start client 2
        System.Environment.SetEnvironmentVariable("IS_DEDICATED_SERVER", "false");
        client2GO = Object.Instantiate(networkManagerPrefab);
        yield return new WaitForSeconds(2);

        var client2NM = client2GO.GetComponent<NetworkManager>();
        Assert.IsTrue(client2NM.ClientManager.Connected, "[CLIENT 2] Client should be connected.");

        int connectedClients = serverNM.ServerManager.Clients.Count;
        Assert.AreEqual(2, connectedClients, $"[SERVER] Expected 2 clients connected, but got {connectedClients}.");
    }
}