using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine.Networking;
using GameKit.Dependencies.Utilities;
using FishNet.Object;

public class MapLoader : MonoBehaviour
{
    public string mapFileName;
    public List<GameObject> prefabs = new();
    private Dictionary<string, GameObject> prefabDictionary = new();
    private bool isOnServer;
    LobbyManager lobbyManager;

    void Awake()
    {
        // check if on server (for networked object)
        isOnServer = Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") == "true";

        // load prefabs into a dictionary
        foreach (GameObject prefab in prefabs)
            prefabDictionary.Add(prefab.name, prefab);
    }

    void Start()
    {
        lobbyManager = FindFirstObjectByType<LobbyManager>();
        StartCoroutine(LoadMapFromFile());
        string path = Path.Combine(Application.streamingAssetsPath, mapFileName);
        if (!File.Exists(path)) {
            if (!isOnServer)
                Debug.LogError($"JSON map file not found : {path}");
            else
                Debug.Log($"[ERROR] JSON map file not found : {path}");
            return;
        }

        // tell the server we are ready
        try
            { FindFirstObjectByType<LobbyManager>().IsReady(); }
        catch (NullReferenceException)
            { Debug.LogError("LobbyManager not found"); }
    }

    private IEnumerator LoadMapFromFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, mapFileName);
        
#if UNITY_ANDROID
            Debug.Log("Running on Android. Attempting to load .env in map loader using UnityWebRequest.");
            yield return StartCoroutine(LoadMapFromAndroid(path));
#else
        Debug.Log("Running on PC. Attempting to load .env in map loader using File.ReadAllText.");
        if (!File.Exists(path)) {
            if (!isOnServer)
                Debug.LogError($"JSON map file not found : {path}");
            else
                Debug.Log($"[ERROR] JSON map file not found : {path}");
            yield break;
        }
        //Load the map from the name of the file
        LoadMap(File.ReadAllText(path));
        yield return null;
#endif 
    }

    private IEnumerator LoadMapFromAndroid(string path)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(path))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonString = request.downloadHandler.text;
                LoadMap(jsonString);
            }
            else
            {
                Debug.LogError($"Failed to load JSON map file: {request.error}");
            }
        }
    }

    void LoadMap(string jsonString)
    {
        MapData mapData = JsonConvert.DeserializeObject<MapData>(jsonString);
        
        // camera
        InstantiateCamera(mapData.mainCamera);

        
        // map objects
        GameObject map = new() { name = "map" }; // create new empty that will hold every object

        foreach (GameObjectJson objectJson in mapData.objects)
            InstantiatePrefab(objectJson, map.transform);

        
        // spawnPoints
        GameObject playerSpawns = new() { name = "PlayerSpawns" }; // create new empty that will hold every spawnpoints
        
        foreach (SerializableVector3 spawnPosition in mapData.spawnPoints)
            InstantiateSpawnpoint(spawnPosition, playerSpawns.transform);
    }

    void InstantiateCamera(CameraJson cameraData) {
        // create new empty
        GameObject cam = new() {
            name = "Main Camera",
            tag = "MainCamera"
        };

        // set saved parameters
        Camera cameraComponent = cam.AddComponent<Camera>();
        cam.transform.SetPositionAndRotation(cameraData.position.ToVector3(), Quaternion.Euler(cameraData.rotation.ToVector3()));
        cam.transform.SetScale(cameraData.scale.ToVector3());
        cameraComponent.fieldOfView = cameraData.fieldOfView;
        cameraComponent.nearClipPlane = cameraData.nearClipPlane;
        cameraComponent.farClipPlane = cameraData.farClipPlane;

        // set audioListener
        if (cameraData.hasAudioListener)
            cam.AddComponent<AudioListener>();

        // set additional camera
        if (cameraData.isAdditionalCamera)
            cam.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    }

    void InstantiatePrefab(GameObjectJson goData, Transform parent) {
        // search prefab
        GameObject prefab = prefabDictionary[goData.prefabName];

        // instantiate it
        GameObject newObject = Instantiate(prefab, parent);
        newObject.transform.position = goData.position.ToVector3();

        // if networked, spawn it
        if (goData.isNetworked && isOnServer)
            lobbyManager.GetComponent<NetworkObject>().Spawn(newObject);
        
        // spawn children
        foreach (GameObjectJson childrenData in goData.children)
            InstantiatePrefab(childrenData, newObject.transform);
    }

    void InstantiateSpawnpoint(SerializableVector3 vec3, Transform parent) {
        // create new empty
        GameObject newSpawnPoint = new() { name = "SpawnPoint" };
        newSpawnPoint.transform.parent = parent;
        newSpawnPoint.transform.position = vec3.ToVector3();
    }
}
