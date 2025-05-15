using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

//Object that breaks after a certain number of lives, and changes texture/material with each life
public class BreakableObject : NetworkBehaviour
{
    private readonly SyncVar<int> health = new(new SyncTypeSettings(1f)); // Number of hits required to break the object

    public int nbStartingLife = 3;

    public Mesh[] meshes;
    public Color[] colors;
    private MeshFilter activeMesh;
    private MeshRenderer meshRenderer;
    private Rigidbody rb;

    private void Awake()
    {
        activeMesh = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (activeMesh == null || meshRenderer == null)
        {
            Debug.LogError("Renderer not found on " + gameObject.name);
        }

        health.Value = nbStartingLife;
        health.OnChange += OnHealthChange;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // rb.isKinematic = true;
            // rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        UpdateVisualState(health.Value, health.Value, true); // Apply the correct texture at the start
    }

    public void TakeDamage() {
        health.Value--;
    }

    public void OnHealthChange(int previousHealth, int nextHealth, bool asServer)
    {
        if (nextHealth == 0)
        {
            if (base.IsServerInitialized)
                Despawn(gameObject);
        }

        UpdateVisualState(previousHealth, nextHealth, true);
    }

    void UpdateVisualState(int oldHealth, int newHealth, bool asServer)
    {
        if (newHealth > 0 && newHealth <= meshes.Length)
        {
            activeMesh.mesh = meshes[newHealth - 1]; // Change the mesh
            meshRenderer.material.color = colors[newHealth - 1]; // Change the material color
        }
    }

}
