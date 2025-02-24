using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Create an empty GameObject that is used to group the bushes together, and manage the transparency of the whole group
public class BushGroup : MonoBehaviour
{
    private List<BushObject> bushes = new List<BushObject>();

    private void Start()
    {
        // Collect all bushes in this group
        foreach (Transform child in transform)
        {
            BushObject bush = child.GetComponent<BushObject>();
            if (bush != null)
            {
                bushes.Add(bush);
            }
        }
    }

    //Change transparency for the whole group
    public void SetTransparencyForGroup(float alpha)
    {
        foreach (BushObject bush in bushes)
        {
            bush.SetTransparency(alpha);
        }
    }
}
