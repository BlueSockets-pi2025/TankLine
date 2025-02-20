using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using FishNet.Object;
// using FishNet.Object.Synchronizing;

public class BreakableObject : MonoBehaviour
// public class BreakableObject : NetworkBehaviour
{
    // [SyncVar(OnChange = nameof(UpdateVisualState))]
    public int health = 3; // Number of hits required to break the object

    public Material[] textures; // Texture or Material table (3 = intact, 2 = damaged, 1 = almost broken)
    private Renderer objRenderer;

    private void Start()
    {
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
            // if (IsServer) // Only the server handles damage
            // {
            TakeDamage();
            // }
        }
    }

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
