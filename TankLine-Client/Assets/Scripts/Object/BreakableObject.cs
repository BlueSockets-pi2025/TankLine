using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using FishNet.Object;
// using FishNet.Object.Synchronizing;

//Object that breaks after a certain number of lives, and changes texture/material with each life
public class BreakableObject : MonoBehaviour
// à mettre quand ajout de FishNet/multi
// public class BreakableObject : NetworkBehaviour
{
    // à mettre quand ajout de FishNet/multi
    // [SyncVar(OnChange = nameof(UpdateVisualState))]
    public int health = 3; // Number of hits required to break the object

    public Material[] textures; // Texture (public Texture[]) or Material (public Material[]) table to represent the number of lives remaining
    private Renderer objRenderer;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        objRenderer = GetComponent<Renderer>();
        if (objRenderer == null)
        {
            Debug.LogError("Renderer not found on " + gameObject.name);
        }
        UpdateVisualState(health, health, true); // Apply the correct texture at the start
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object was hit by a projectile
        if (other.CompareTag("TankShell"))
        {
            // à mettre quand ajout de FishNet/multi
            // if (IsServer) // Only the server handles damage
            // {
            TakeDamage();
            // }
        }
    }

    // à mettre quand ajout de FishNet/multi
    // [Server] // Ensures only the server modifies health
    void TakeDamage()
    {
        if (health > 0)
        {
            var oldHealth = health;
            health--;
            UpdateVisualState(oldHealth, health, true);
            Debug.Log($"{gameObject.name} health: {health}");
            if (health == 0)
            {
                RpcBreak(); // Destroys the object on all clients
            }
        }
    }

    // à mettre quand ajout de FishNet/multi
    // [ObserversRpc]
    void RpcBreak()
    {
        Debug.Log($"{gameObject.name} has been broken!");
        Destroy(gameObject);
    }

    void UpdateVisualState(int oldHealth, int newHealth, bool asServer)
    {
        if (newHealth > 0 && newHealth <= textures.Length)
        {
            // objRenderer.material.mainTexture = textures[newHealth - 1]; // Change the texture
            objRenderer.material = textures[newHealth - 1]; // Change the material

        }
    }

}
