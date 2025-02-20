using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using FishNet.Object;

// Creation of a bush belonging to a BushGroup which becomes transparent on contact.
public class BushObject : MonoBehaviour
// à mettre quand ajout de FishNet/multi
// public class BushObject : NetworkBehaviour
{
    private Renderer bushRenderer;
    private Color originalColor;
    public float transparency = 0.5f; // Opacity level when inside

    private BushGroup parentGroup;

    private void Start()
    {
        // Get the Renderer component
        bushRenderer = GetComponent<Renderer>();

        if (bushRenderer == null)
        {
            Debug.LogError("Renderer not found on " + gameObject.name);
            return;
        }

        // Store the original color
        originalColor = bushRenderer.material.color;

        // Find the parent group
        parentGroup = GetComponentInParent<BushGroup>();
        if (parentGroup == null)
        {
            Debug.LogError("BushGroup not found for " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's a player and if it's the local player (FishNet)
        // à mettre quand ajout de FishNet/multi
        // if (other.CompareTag("Player") && other.TryGetComponent(out NetworkObject netObj) && netObj.IsOwner)
        if (other.CompareTag("Player"))
        {
            parentGroup?.SetTransparencyForGroup(transparency);
            // SetTransparency(transparency);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if it's the local player
        // à mettre quand ajout de FishNet/multi
        // if (other.CompareTag("Player") && other.TryGetComponent(out NetworkObject netObj) && netObj.IsOwner)
        if (other.CompareTag("Player"))
        {
            // SetTransparency(1f); // Restore full opacity
            parentGroup?.SetTransparencyForGroup(1f);
        }
    }

    // Function called by the BushGroup class to change the transparency of the bush
    public void SetTransparency(float alpha)
    {
        if (bushRenderer != null)
        {
            Color newColor = originalColor;
            newColor.a = alpha;
            bushRenderer.material.color = newColor;
        }
    }
}
