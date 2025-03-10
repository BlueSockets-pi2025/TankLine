using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public List<GameObject> prefabList;
    public Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();


    void Awake()
    {
        prefabDictionary = new Dictionary<string, GameObject>();
        foreach (var prefab in prefabList)
        {
            prefabDictionary[prefab.name] = prefab;
        }
    }
}
