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
    private float transparencyGroup = 0.6f; // Opacity level when inside
    private float transparencyOne = 0.2f; // Opacity level when inside
    private float fadeDuration = 0.15f; // Duration of the fade effect

    private Coroutine fadeCoroutine = null;

    private BushGroup parentGroup;

    // Store the list of all bushes in contact with the player
    private static Dictionary<GameObject, HashSet<BushObject>> playerBushes = new Dictionary<GameObject, HashSet<BushObject>>();


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
            // Add this bush to the player's bush list
            if (!playerBushes.ContainsKey(other.gameObject))
            {
                playerBushes[other.gameObject] = new HashSet<BushObject>();
            }
            playerBushes[other.gameObject].Add(this);
            parentGroup?.SetTransparencyForGroup(transparencyGroup);

            // Update all bushes in contact with the player
            foreach (BushObject bush in playerBushes[other.gameObject])
            {
                bush.SetTransparency(transparencyOne);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if it's the local player
        // à mettre quand ajout de FishNet/multi
        // if (other.CompareTag("Player") && other.TryGetComponent(out NetworkObject netObj) && netObj.IsOwner)
        if (other.CompareTag("Player"))
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
                // if (playerBushes[other.gameObject].Count == 0)
                {
                    parentGroup?.SetTransparencyForGroup(1f); // Restore full opacity
                }
                else
                {
                    parentGroup?.SetTransparencyForGroup(transparencyGroup);
                    // Update all bushes in contact with the player
                    foreach (BushObject bush in playerBushes[other.gameObject])
                    {
                        bush.SetTransparency(transparencyOne);
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
    public void SetTransparency(float targetAlpha)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeTransparency(targetAlpha));
    }

    private IEnumerator FadeTransparency(float targetAlpha)
    {
        if (bushRenderer != null)
        {
            Color currentColor = bushRenderer.material.color;
            float startAlpha = currentColor.a;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                currentColor.a = newAlpha;
                bushRenderer.material.color = currentColor;
                yield return null;
            }

            // Ensure final value is set correctly
            currentColor.a = targetAlpha;
            bushRenderer.material.color = currentColor;
        }
    }
}
