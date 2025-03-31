using System.Collections.Generic;
using UnityEngine;

//To create a list of all prefabs to load into the scene.
public class PrefabManager : MonoBehaviour
{
    public List<GameObject> prefabList;
    public Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();


    void Awake()
    {
        //Adds each prefab placed in the Unity Editor inspector to a list or dictionary.
        prefabDictionary = new Dictionary<string, GameObject>();
        foreach (var prefab in prefabList)
        {
            prefabDictionary[prefab.name] = prefab;
        }
    }
}
