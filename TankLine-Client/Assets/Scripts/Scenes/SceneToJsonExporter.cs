using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using FishNet.Object;
using GameKit.Dependencies.Utilities;

/// <summary>
/// Export a map scene into JSON <br/>
/// Map hierarchy :  <br/> <br/>
/// "Map"  <br/>
///     -> "object 1"  <br/>
///     -> "object 2"  <br/>
/// "PlayerSpawns"  <br/>
///     -> "spawnPoint 1"  <br/>
///     -> "spawnPoint 2"  <br/>
/// "Main Camera"  <br/>
/// </summary>
public class SceneToJsonExporter : MonoBehaviour
{
    // You can also change the name in the Unity Editor when saving. 
    public string mapName = "NameScene.json";

    /// <summary>
    /// Export a whole map as a Json object 
    /// </summary>
    public void ExportMap() {
        MapData mapData = new() { name = mapName };

        // get every map object
        Transform mapTransform = GameObject.Find("map").transform;
        if (mapTransform == null) Debug.LogError("Error, map object is null");

        // add them to the mapData
        foreach (Transform objectTf in mapTransform)
            mapData.objects.Add(ExportGameObjectToJson(objectTf.gameObject));


        // get every spawnPoint
        Transform spawnPointsTransform = GameObject.Find("PlayerSpawns").transform;
        if (spawnPointsTransform == null) Debug.LogError("Error, PlayerSpawns object is null");

        // add them to the mapData
        foreach (Transform objectTf in spawnPointsTransform)
            mapData.spawnPoints.Add(SerializableVector3.FromVector3(objectTf.position));

        mapData.mainCamera = ExportMainCameraJson();

        string json = JsonConvert.SerializeObject(mapData, Formatting.Indented);
        string path = Path.Combine(Application.streamingAssetsPath, mapName);

        File.WriteAllText(path, json);

        Debug.Log($"[JSON EXPORT] Scene exported correctly : {Path.Combine(Application.streamingAssetsPath, mapName)}");
    }

    /// <summary>
    /// Export the main camera into a `CameraJson`
    /// </summary>
    /// <returns>The CameraJson structure corresponding</returns>
    CameraJson ExportMainCameraJson() {
        Camera camera = Camera.main;

        return new() {
            farClipPlane = camera.farClipPlane,
            fieldOfView = camera.fieldOfView,
            hasAudioListener = camera.GetComponent<AudioListener>() != null,
            nearClipPlane = camera.nearClipPlane,
            isAdditionalCamera = GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() != null,
            position = SerializableVector3.FromVector3(camera.transform.position),
            rotation = SerializableVector3.FromVector3(camera.transform.rotation.eulerAngles),
            scale = SerializableVector3.FromVector3(camera.transform.GetScale())
        };
    }

    /// <summary>
    /// Export a single gameObject (and his children) into a `GameObjectJson`
    /// </summary>
    /// <param name="go">The gameObject to export</param>
    /// <returns>The GameObjectJson structure corresponding</returns>
    GameObjectJson ExportGameObjectToJson(GameObject go) {
        GameObjectJson data = new() {
            position = SerializableVector3.FromVector3(go.transform.localPosition),
            isNetworked = go.GetComponent<NetworkObject>() == true,
            prefabName = GetPrefabName(go)
        };

        // add every children
        foreach (Transform objectTf in go.transform)
            data.children.Add(ExportGameObjectToJson(objectTf.gameObject));

        return data;
    }

    //To get the prefab's name instead of the name of the object in the scene (to match with the prefab when loading the scene).
    string GetPrefabName(GameObject obj)
    {
        //Only available when you are using Unity Editor.
#if UNITY_EDITOR
        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
        if (prefab != null)
        {
            return prefab.name; // Returns the name of the prefab instead of the name of the object in the scene
        }
#endif
        return obj.name; // If it is not a prefab, we keep its current name
    }
}

//To have the button to export in the Unity Editor only.
#if UNITY_EDITOR
[CustomEditor(typeof(SceneToJsonExporter))]
public class SceneToJsonExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SceneToJsonExporter script = (SceneToJsonExporter)target;
        if (GUILayout.Button("Export Scene to JSON"))
        {
            script.ExportMap();
        }
    }
}
#endif