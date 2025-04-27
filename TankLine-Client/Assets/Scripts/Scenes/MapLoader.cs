using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;

public class MapLoader : MonoBehaviour
{
    public string mapFileName;
    public PrefabManager prefabManager;
    public Dictionary<string, GameObject> prefabDictionary;

    //"save&load.test" is the name of an AssetBundles. We need to create one to bundle all the files needed for loading.
#if UNITY_EDITOR
    // In Editor mode, load from an external folder.
    static string bundlePath = Path.Combine(Application.dataPath, "../AssetBundles/save&load.test");
#else
    // In Build, load from StreamingAssets (we need to move the folder "AssetBundles" in the folder "StreamingAssets").
    static string bundlePath = Path.Combine(Application.streamingAssetsPath, "AssetBundles/save&load.test");
#endif

    private AssetBundle myBundle;

    private bool isOnServer;

    void Awake()
    {
        isOnServer = Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") == "true";
    }

    void Start()
    {
        //Path verification
        if (prefabManager == null)
        {
            Debug.LogError("PrefabManager is not assigned.");
        }
        if (prefabManager.prefabDictionary.Count == 0)
        {
            Debug.LogError("The prefabs dictionary is empty.");
        }
        myBundle = AssetBundle.LoadFromFile(bundlePath);
        if (myBundle == null)
        {
            Debug.LogError("AssetBundle failed to load.");
            return;
        }
        //Load the map from the name of the file
        LoadMap(mapFileName);

        // tell the server we are ready
        FindFirstObjectByType<LobbyManager>().IsReady();
    }

    void LoadMap(string fileName)
    {
        //Load the JSON file from the StreamingAssets folder.
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            MapData map = JsonConvert.DeserializeObject<MapData>(json);

            //Foreach objects in the JSON file, instantiate the object. (Reminder: these are only the parent objects)
            foreach (var objData in map.objects)
            {
                InstantiateObject(objData, null);
            }
        }
        else
        {
            Debug.LogError("JSON file not found: " + path);
        }
    }

    //To Instantiate any Object
    GameObject InstantiateObject(GameObjectData objData, Transform parent)
    {
        GameObject prefab = null;

        // Checks if a prefab exists in the dictionary (see PrefabManager.cs).
        if (prefabManager.prefabDictionary.ContainsKey(objData.prefabName))
        {
            //Takes the existing prefab.
            prefab = prefabManager.prefabDictionary[objData.prefabName];
        }
        else
        {
            //Search if there is a 3D Model.
            if (!isOnServer) {
                prefab = LoadModelFromResources(objData.modelName);

                // If no model is found in the resources, creates a base Unity object.
                if (prefab == null)
                {
                    prefab = CreatePrimitiveObject(objData.modelName);
                    //CreatePrimitive creates a GameObjectEmpty. Destroy this GameObject to instantiate only the one needed.
                    Destroy(prefab);
                }
            }
        }

        // Check that the prefab is valid.
        if (prefab == null)
        {
            Debug.LogError("Prefab is null for: " + objData.prefabName);
            return null;
        }

        //Instantiate the object.
        GameObject instance = Instantiate(prefab, objData.position.ToVector3(), Quaternion.Euler(objData.rotation.ToVector3()), parent);
        //Set the name to the prefabName of the original scene and not the name of the prefab to be instantiated.
        instance.name = objData.prefabName;
        instance.transform.localScale = objData.scale.ToVector3();
        instance.transform.SetParent(parent);
        instance.tag = objData.tag;

        //If the gameObject is a camera, instantiate the data.
        if (objData.cameraData != null)
        {
            Camera cam = instance.AddComponent<Camera>();
            cam.fieldOfView = objData.cameraData.fieldOfView;
            cam.nearClipPlane = objData.cameraData.nearClipPlane;
            cam.farClipPlane = objData.cameraData.farClipPlane;

            if (objData.cameraData.hasAudioListener)
            {
                instance.AddComponent<AudioListener>();
            }

            if (objData.cameraData.isAdditionalCamera)
            {
                instance.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            }
        }

        //If the gameObject is a light, instantiate the data.
        if (objData.lightData != null)
        {
            Light light = instance.AddComponent<Light>();
            light.type = objData.lightData.type;
            light.intensity = objData.lightData.intensity;
            light.color = objData.lightData.color.ToColor();
            light.shadows = objData.lightData.shadows ? LightShadows.Soft : LightShadows.None;

            if (objData.lightData.isAdditionalLight)
            {
                instance.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
            }
        }

        //Foreach scripts, if there is any, add them to the gameObject.
        foreach (string scriptName in objData.scripts)
        {
            Type scriptType = Type.GetType(scriptName);
            if (scriptType != null)
            {
                if (instance.GetComponent(scriptType) == null)
                {
                    instance.AddComponent(scriptType);
                }
            }
            else
            {
                Debug.LogWarning("Script not found: " + scriptName);
            }
        }

        //Apply the material if there is one.
        //ApplyMaterial(instance, objData.materialName);

        //Instantiate all the children, if there is any.
        foreach (var childData in objData.children)
        {
            if (childData != null)
            {
                InstantiateObject(childData, instance.transform);
            }
        }

        return instance;
    }

    //Load a model from resources.
    GameObject LoadModelFromResources(string modelName)
    {
        if (!string.IsNullOrEmpty(modelName) && myBundle != null)
        {
            //Try loading the model from the "Resources/3D_Models/" folder
            GameObject model = myBundle.LoadAsset<GameObject>(modelName);
            if (model != null)
            {
                return model;
            }
            else
            {
                // Debug.LogWarning("Model not found in resources: " + modelName);
            }
        }
        return null;
    }

    //Create a basic Unity object (Cube, Sphere, etc.) or an EmptyGameObject
    GameObject CreatePrimitiveObject(string modelName)
    {
        if (!string.IsNullOrEmpty(modelName))
        {
            switch (modelName.ToLower())
            {
                case "cube":
                    return GameObject.CreatePrimitive(PrimitiveType.Cube);
                case "sphere":
                    return GameObject.CreatePrimitive(PrimitiveType.Sphere);
                case "cylinder":
                    return GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                case "plane":
                    return GameObject.CreatePrimitive(PrimitiveType.Plane);
                case "capsule":
                    return GameObject.CreatePrimitive(PrimitiveType.Capsule);
                default:
                    break;
            }
        }

        //If the model is not recognized or found, an EmptyGameObject is created
        return new GameObject(modelName);
    }

    //Applies a material to the instance if a material name is provided
    void ApplyMaterial(GameObject instance, string materialName)
    {
        Renderer renderer = instance.GetComponent<Renderer>();
        if (renderer && !string.IsNullOrEmpty(materialName) && myBundle != null)
        {
            Material material = myBundle.LoadAsset<Material>(materialName);

            if (material != null)
            {
                renderer.material = material;
            }
            else
            {
                // Debug.LogWarning("Material not found: " + materialName);
            }
        }
    }
}
