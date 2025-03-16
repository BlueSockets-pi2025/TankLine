using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;

//To save position, rotation, and size.
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

//Name of the file/Scene.
[System.Serializable]
public class MapData
{
    public string name;
    public List<GameObjectData> objects = new List<GameObjectData>();
}

//To save the data of the camera.
[System.Serializable]
public class CameraData
{
    public float fieldOfView;
    public float nearClipPlane;
    public float farClipPlane;
    public bool hasAudioListener;
    public bool isAdditionalCamera; // URP Support
}

//To save the color of the light if needed.
[System.Serializable]
public class SerializableColor
{
    public float r, g, b, a;

    public SerializableColor(Color color)
    {
        r = color.r;
        g = color.g;
        b = color.b;
        a = color.a;
    }

    public Color ToColor()
    {
        return new Color(r, g, b, a);
    }
}

//To save the data of the light.
[System.Serializable]
public class LightData
{
    public LightType type;
    public float intensity;
    public SerializableColor color;
    public bool shadows;
    public bool isAdditionalLight; // URP Support
}

//To save the data of any object (parameters in JSON).
[System.Serializable]
public class GameObjectData
{
    public string prefabName;
    public string materialName;
    public string modelName;
    public string tag;
    public SerializableVector3 position;
    public SerializableVector3 rotation;
    public SerializableVector3 scale;
    public List<string> scripts = new List<string>();
    public List<GameObjectData> children = new List<GameObjectData>();
    public CameraData cameraData;
    public LightData lightData;
}

public class SceneToJsonExporter : MonoBehaviour
{
    // You can also change the name in the Unity Editor when saving. 
    public string mapName = "NameScene.json";

    //Foreach of the GameObjects in the Scene who are root objects, export the object annd put them in JSON
    public void ExportScene()
    {
        MapData mapData = new MapData { name = mapName };
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        List<GameObjectData> objects = new List<GameObjectData>();

        foreach (GameObject obj in rootObjects)
        {
            if (obj.transform.parent == null) //Take only root objects
            {
                mapData.objects.Add(ExportGameObject(obj));
            }
        }

        string json = JsonConvert.SerializeObject(mapData, Formatting.Indented);
        string path = Path.Combine(Application.streamingAssetsPath, mapName + ".json");

        File.WriteAllText(path, json);
    }


    //Export all the parameters of the GameObject.
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
            children = new List<GameObjectData>(),
            scripts = new List<string>(),
            tag = obj.tag
        };

        //If the gameObject is a camera, export its parameters, and data if there is any.
        Camera cam = obj.GetComponent<Camera>();
        if (cam != null)
        {
            objData.cameraData = new CameraData
            {
                fieldOfView = cam.fieldOfView,
                nearClipPlane = cam.nearClipPlane,
                farClipPlane = cam.farClipPlane,
                hasAudioListener = obj.GetComponent<AudioListener>() != null
            };

            //Checks if the object has UniversalAdditionalCameraData (URP)
            var additionalCamData = obj.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (additionalCamData != null)
            {
                objData.cameraData.isAdditionalCamera = true;
            }
        }

        //If the gameObject is a light, export its parameters, and data if there is any.
        Light light = obj.GetComponent<Light>();
        if (light != null)
        {
            objData.lightData = new LightData
            {
                type = light.type,
                intensity = light.intensity,
                color = new SerializableColor(light.color),
                shadows = light.shadows != LightShadows.None
            };

            var additionalLightData = obj.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
            if (additionalLightData != null)
            {
                objData.lightData.isAdditionalLight = true;
            }
        }

        //If there are scripts, add them to a list.
        foreach (MonoBehaviour script in obj.GetComponents<MonoBehaviour>())
        {
            if (script != null)
            {
                objData.scripts.Add(script.GetType().Name);
            }
        }

        //If there are children, export them too (by recursive).
        foreach (Transform child in obj.transform)
        {
            objData.children.Add(ExportGameObject(child.gameObject));
        }

        return objData;
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
            script.ExportScene();
        }
    }
}
#endif