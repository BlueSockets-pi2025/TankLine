using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Clouds : MonoBehaviour
{
    public List<GameObject> cloudsPrefabs;

    public List<GameObject> currentClouds = new List<GameObject>();

    void Start() {

        // check if the script is running on the server with the env variable "IS_DEDICATED_SERVER"
        string isDedicatedServer = Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER");

        // if on dedicated server, disable cloud for better performance
        if (String.Compare(isDedicatedServer, "true") == 0)
            gameObject.GetComponent<Clouds>().enabled = false;
    }

    void Update() {

        // make cloud scroll
        for (int i=currentClouds.Count-1; i>=0; i--) {
            GameObject cloud = currentClouds[i];

            // if has already scrolled through the whole screen, remove
            if (cloud.transform.position.x >= 25f) {
                currentClouds.RemoveAt(i);
                Destroy(cloud);
            } 
            // else, scroll right
            else {
                cloud.transform.Translate((cloud.transform.localPosition.y) * 3 * Time.deltaTime * Vector3.right);
            }
        }

        // every third of second, try to spawn new clouds
        if (Time.frameCount % 10 == 0) {
            System.Random random = new();

            // 66% of chance to spawn a new cloud
            if (random.Next(101) < 66) {
                GameObject newCloud = Instantiate(cloudsPrefabs[random.Next(cloudsPrefabs.Count)], gameObject.transform);

                // random Y position (in [5, 1]), random Z position (in [-14, 14])
                newCloud.transform.localPosition = new Vector3(-25f, random.Next(41) / 10f + 1f, (random.Next(281) / 10f) - 14f);

                // random scale (in [0.5, 1.5])
                newCloud.transform.localScale *= newCloud.transform.localPosition.y / 4f;

                currentClouds.Add(newCloud);
            }
        }
    }
}
