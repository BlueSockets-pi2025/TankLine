using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class MapLoader : MonoBehaviour
{
    public string mapFileName;
    public PrefabManager prefabManager;
    public Dictionary<string, GameObject> prefabDictionary;
    void Start()
    {
        if (prefabManager == null)
        {
            Debug.LogError("PrefabManager n'est pas assigné!");
        }
        if (prefabManager.prefabDictionary.Count == 0)
        {
            Debug.LogError("Le dictionnaire de prefabs est vide!");
        }
        LoadMap(mapFileName);
    }

    void LoadMap(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            MapData map = JsonConvert.DeserializeObject<MapData>(json);
            Debug.Log(map.objects.Count);

            foreach (var objData in map.objects)
            {
                InstantiateObject(objData, null);
                Debug.Log("!!!!!!!!!!!!!!!!!!!!!!! objData : " + objData.prefabName);
            }
        }
        else
        {
            Debug.LogError("Fichier JSON non trouvé : " + path);
        }
    }

    GameObject InstantiateObject(GameObjectData objData, Transform parent)
    {
        GameObject prefab = null;

        // Vérifie si un prefab existe dans le dictionnaire
        if (prefabManager.prefabDictionary.ContainsKey(objData.prefabName))
        {
            prefab = prefabManager.prefabDictionary[objData.prefabName];
        }
        else
        {
            prefab = LoadModelFromResources(objData.modelName);

            // Si aucun modèle n'est trouvé dans les ressources, crée un objet Unity de base
            if (prefab == null)
            {
                prefab = CreatePrimitiveObject(objData.modelName);
                // prefab = new GameObject(objData.modelName);
            }
        }

        // Vérifie que le prefab est valide
        if (prefab == null)
        {
            // Debug.LogError("Prefab est null pour : " + objData.prefabName);
            return null;
        }

        Debug.Log("Instanciation de : " + objData.prefabName);
        //Instancier l'objet
        GameObject instance = Instantiate(prefab, objData.position.ToVector3(), Quaternion.Euler(objData.rotation.ToVector3()), parent);
        // Debug.Log("Prefab instancié : " + instance.name);
        instance.name = objData.prefabName;
        instance.transform.localScale = objData.scale.ToVector3();
        instance.transform.SetParent(parent);

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

        if (objData.isAudioVolume)
        {
            instance.AddComponent<UnityEngine.Rendering.Volume>();
        }


        foreach (string scriptName in objData.scripts)
        {
            System.Type scriptType = System.Type.GetType(scriptName);
            if (scriptType != null)
            {
                if (instance.GetComponent(scriptType) == null)
                {
                    instance.AddComponent(scriptType);
                }
            }
            else
            {
                Debug.LogWarning("Script non trouvé : " + scriptName);
            }
        }

        // ApplyMaterial(instance, objData.materialName);

        foreach (var childData in objData.children)
        {
            if (childData != null)
            {
                Debug.Log("Enfant instancié : " + childData.prefabName);
                InstantiateObject(childData, instance.transform);
            }
        }

        return instance;
    }

    // Méthode pour charger un modèle à partir des ressources
    GameObject LoadModelFromResources(string modelName)
    {
        if (!string.IsNullOrEmpty(modelName))
        {
            // Essaye de charger le modèle à partir du dossier Resources/Models/
            GameObject model = Resources.Load<GameObject>("3D_Models/" + modelName);
            if (model != null)
            {
                Debug.Log("Modèle trouvé dans les ressources : " + modelName);
                return model;
            }
            else
            {
                // Debug.LogWarning("Modèle non trouvé dans les ressources : " + modelName);
            }
        }
        return null;
    }

    // Méthode pour créer un objet Unity de base (Cube, Sphère, etc.)
    GameObject CreatePrimitiveObject(string modelName)
    {
        if (!string.IsNullOrEmpty(modelName))
        {
            switch (modelName.ToLower()) // Utilise ToLower pour gérer la casse
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
                    Debug.LogWarning("Modèle inconnu : " + modelName);
                    break;
            }
        }

        // Si le modèle n'est pas reconnu ou trouvé, on crée un objet vide
        // pb si juste gameobjectvide donc parent, ou si pas reconnu, mis en double ??
        Debug.LogWarning("Modèle inconnu pas deux fois " + modelName);
        return new GameObject(modelName);
    }

    // Applique un matériau à l'instance si un nom de matériau est fourni
    void ApplyMaterial(GameObject instance, string materialName)
    {
        Renderer renderer = instance.GetComponent<Renderer>();
        if (renderer && !string.IsNullOrEmpty(materialName))
        {
            Material material = Resources.Load<Material>("Materials/" + materialName);
            if (material != null)
            {
                renderer.material = material;
            }
            else
            {
                Debug.LogWarning("Matériau non trouvé : " + materialName);
            }
        }
    }
}
