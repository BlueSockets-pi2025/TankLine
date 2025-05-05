using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.Formats.Alembic.Importer;
using System.Collections;

//Object that breaks after a certain number of lives, and changes texture/material with each life
public class BreakableObject : NetworkBehaviour
{
    private readonly SyncVar<int> health = new(new SyncTypeSettings(1f)); // Number of hits required to break the object

    private AlembicStreamPlayer alembicPlayer;

    public int nbStartingLife = 3;
    public float speed = 1f;

    public float damage_time_1 = 0f;
    public float damage_time_2 = 0f;

    private Renderer objRenderer;
    private Rigidbody rb;

    private void Awake()
    {
        objRenderer = GetComponent<Renderer>();
        alembicPlayer = GetComponent<AlembicStreamPlayer>();
        alembicPlayer.CurrentTime = 0;
        alembicPlayer.enabled = false;

        if (objRenderer == null)
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

    }

    public void TakeDamage() {
        health.Value--;
    }

    public void OnHealthChange(int previousHealth, int nextHealth, bool asServer)
    {
        if (nextHealth == 2)
        {
            StartCoroutine(PerformAnimationPart1());
        }
        else if (nextHealth == 1)
        {
            StartCoroutine(PerformAnimationPart2());
        }
        else if (nextHealth == 0)
        {
            StartCoroutine(PerformAnimationPart3());
        }
    }

    private IEnumerator PerformAnimationPart1()
    {
        alembicPlayer.enabled = true;
        while(alembicPlayer.CurrentTime < (damage_time_1 / speed))
        {
            if (alembicPlayer != null)
            {
                alembicPlayer.CurrentTime += Time.deltaTime*speed;
            }
            yield return null;
        }
        alembicPlayer.enabled = false;
    }

    private IEnumerator PerformAnimationPart2()
    {
        alembicPlayer.enabled = true;
        while(alembicPlayer.CurrentTime < (damage_time_2 / speed))
        {
            if (alembicPlayer != null)
            {
                alembicPlayer.CurrentTime += Time.deltaTime*speed;
            }
            yield return null;
        }
        alembicPlayer.enabled = false;
    }

    private IEnumerator PerformAnimationPart3()
    {
        alembicPlayer.enabled = true;
        while(alembicPlayer.CurrentTime < (alembicPlayer.Duration / speed))
        {
            if (alembicPlayer != null)
            {
                alembicPlayer.CurrentTime += Time.deltaTime*speed;
            }
            yield return null;
        }
        alembicPlayer.enabled = false;
        if (base.IsServerInitialized)
            Despawn(gameObject);
    }

}
