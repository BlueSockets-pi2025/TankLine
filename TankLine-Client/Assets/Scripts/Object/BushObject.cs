using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

// Creation of a bush belonging to a BushGroup which becomes transparent on contact.
public class BushObject : MonoBehaviour
{
    private const int PLAYER_LAYER = 10;
    private Renderer bushRenderer;
    private Color originalColor;
    private float transparencyGroup = 0.6f; // Opacity level when inside
    private float transparencyOne = 0.2f; // Opacity level when inside
    private float fadeDuration = 0.15f; // Duration of the fade effect

    private Coroutine fadeCoroutine = null;

    private BushGroup parentGroup;

    // Store the list of all bushes in contact with the player
    private static Dictionary<GameObject, HashSet<BushObject>> playerBushes = new Dictionary<GameObject, HashSet<BushObject>>();

    public Material Solid;
    public Material Transparent;


    private void Start()
    {
        // Get the Renderer component
        bushRenderer = GetComponent<Renderer>();

        if (bushRenderer == null)
        {
            Debug.LogError("Renderer not found on " + gameObject.name);
            return;
        }

        // Find the parent group
        parentGroup = GetComponentInParent<BushGroup>();
        if (parentGroup == null)
        {
            Debug.LogError("BushGroup not found for " + gameObject.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == PLAYER_LAYER && other.gameObject.GetComponent<NetworkBehaviour>().IsOwner) 
        {
            // Add this bush to the player's bush list
            if (!playerBushes.ContainsKey(other.gameObject))
            {
                playerBushes[other.gameObject] = new HashSet<BushObject>();
            }
            playerBushes[other.gameObject].Add(this);
            parentGroup?.SetTransparentForGroup();

            // Update all bushes in contact with the player
            foreach (BushObject bush in playerBushes[other.gameObject])
            {
                bush.SetTransparent();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == PLAYER_LAYER && other.gameObject.GetComponent<NetworkBehaviour>().IsOwner)
        {
            if (playerBushes.ContainsKey(other.gameObject))
            {
                playerBushes[other.gameObject].Remove(this);

                // Check if there are any remaining bushes belonging to the same parent
                bool stillInSameGroup = false;

                foreach (BushObject bush in playerBushes[other.gameObject])
                {
                    if (bush.parentGroup == this.parentGroup)
                    {
                        stillInSameGroup = true;
                        break;
                    }
                }

                // If the player is no longer in any bushes of the parentGroup, remove transparency
                if (!stillInSameGroup)
                {
                    parentGroup?.SetSolidForGroup(); // Restore full opacity
                }
                else
                {
                    parentGroup?.SetTransparentForGroup();
                    // Update all bushes in contact with the player
                    foreach (BushObject bush in playerBushes[other.gameObject])
                    {
                        bush.SetTransparent();
                    }
                }
                if (playerBushes[other.gameObject].Count == 0)
                {
                    playerBushes.Remove(other.gameObject);
                }
            }
        }
    }

    // Function called by the BushGroup class to change the transparency of the bush
    public void SetTransparent()
    {
        bushRenderer.material = Transparent;
    }

    public void SetSolid()
    {
        bushRenderer.material = Solid;
    }
}
