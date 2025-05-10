using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to store all data of a map into JSON
/// </summary>
[Serializable]
public class MapData
{
    public string name;
    public CameraJson mainCamera;
    public List<GameObjectJson> objects = new();
    public List<SerializableVector3> spawnPoints = new();
}

/// <summary>
/// Used to store Vector3 into JSON
/// </summary>
[Serializable]
public class SerializableVector3
{
    public float x, y, z;
    public static SerializableVector3 FromVector3(Vector3 vector) {
        return new() {
            x = vector.x,
            y = vector.y,
            z = vector.z
        };
    }
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}

/// <summary>
/// Used to store camera data into JSON
/// </summary>
[Serializable]
public class CameraJson
{
    public SerializableVector3 position;
    public SerializableVector3 rotation;
    public SerializableVector3 scale;
    public float fieldOfView;
    public float nearClipPlane;
    public float farClipPlane;
    public bool hasAudioListener;
    public bool isAdditionalCamera; // URP Support
}

/// <summary>
/// Used to store a prefab gameObject into JSON
/// </summary>
[Serializable]
public class GameObjectJson
{
    public string prefabName;
    public bool isNetworked = false;
    public SerializableVector3 position;
    public List<GameObjectJson> children = new();
}