using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;
    public SerializableVector3(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class MapData
{
    public string name;
    public List<GameObjectData> objects = new List<GameObjectData>();
}

[System.Serializable]
public class GameObjectData
{
    public string prefabName;
    public string materialName;
    public string modelName;
    public SerializableVector3 position;
    public SerializableVector3 rotation;
    public SerializableVector3 scale;
    public List<GameObjectData> children = new List<GameObjectData>();
}

public class SceneToJsonExporter : MonoBehaviour
{
    public string mapName = "NewMap.json";

    public void ExportScene()
    {
        MapData mapData = new MapData { name = mapName };
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        List<GameObjectData> objects = new List<GameObjectData>();

        // foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        foreach (GameObject obj in rootObjects)
        {
            if (obj.name == "Main Camera")
            {
                continue; // Ignore la caméra principale
            }
            if (obj.transform.parent == null) // Ne prendre que les objets racines
            {
                mapData.objects.Add(ExportGameObject(obj));
            }
        }

        string json = JsonConvert.SerializeObject(mapData, Formatting.Indented);
        string path = Path.Combine(Application.streamingAssetsPath, mapName + ".json");

        File.WriteAllText(path, json);
        Debug.Log("Scene exported to " + path);
    }


    GameObjectData ExportGameObject(GameObject obj)
    {
        GameObjectData objData = new GameObjectData
        {
            prefabName = GetPrefabName(obj),
            materialName = obj.GetComponent<Renderer>() ? obj.GetComponent<Renderer>().sharedMaterial?.name : "",
            modelName = obj.GetComponent<MeshFilter>() ? obj.GetComponent<MeshFilter>().sharedMesh.name : "Unknown",
            position = new SerializableVector3(obj.transform.position),
            rotation = new SerializableVector3(obj.transform.eulerAngles),
            scale = new SerializableVector3(obj.transform.localScale),
            children = new List<GameObjectData>()
        };

        foreach (Transform child in obj.transform)
        {
            objData.children.Add(ExportGameObject(child.gameObject));
        }

        return objData;
    }
    string GetPrefabName(GameObject obj)
    {
#if UNITY_EDITOR
        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);
        if (prefab != null)
        {
            return prefab.name; // 🔥 Retourne le nom du prefab au lieu du nom de l'objet dans la scène
        }
#endif
        return obj.name; // Si ce n'est pas un prefab, on garde son nom actuel
    }
}

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
            script.ExportScene();
        }
    }
}
#endif