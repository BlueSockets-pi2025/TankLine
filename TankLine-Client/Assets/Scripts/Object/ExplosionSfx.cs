#if UNITY_STANDALONE

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Formats.Alembic.Importer;


public class ExplosionSfx : MonoBehaviour
{
    private List<Material> allMaterials = new List<Material>();
    private AlembicStreamPlayer alembicPlayer;
    public float speed = 0.5f;
    private float timer = 0f;
    private Renderer rend;
    private bool finished;

    void Awake()
    {
        // Récupère tous les renderers (MeshRenderer + SkinnedMeshRenderer)
        var renderers = GetComponentsInChildren<Renderer>();

        foreach (var rend in renderers)
        {
            foreach (var mat in rend.sharedMaterials)
            {
                if (mat != null && !allMaterials.Contains(mat))
                {
                    allMaterials.Add(mat);
                    mat.SetFloat("Fade", 1f);
                }
            }

        }

        alembicPlayer = GetComponent<AlembicStreamPlayer>();
        alembicPlayer.CurrentTime = 0;
        finished = false;

        //StartCoroutine(PreloadAlembic());
        play();
    }

    private IEnumerator PreloadAlembic()
    {
        alembicPlayer.CurrentTime = alembicPlayer.Duration;
        yield return null; // attend une frame pour forcer le chargement
        alembicPlayer.CurrentTime = 0;
        play();
    }

    public void play()
    {
        foreach (Material mat in allMaterials)
        {
            if (mat.HasProperty("Fade"))
            {
                mat.SetFloat("Fade", 1f);
            }
        }
        finished = false;
        StartCoroutine(PerformAnimation());

    }

    protected void Update()
    {
        if (finished)
        {
            finished = false;
            alembicPlayer.CurrentTime = 0f;

            foreach (Material mat in allMaterials)
            {
                if (mat.HasProperty("Fade"))
                {
                    mat.SetFloat("Fade", 1f);
                }
            }
            Destroy(gameObject);
        }
    }

    private IEnumerator PerformAnimation()
    {
        while (alembicPlayer.CurrentTime < (alembicPlayer.Duration / speed))
        {
            if (alembicPlayer != null)
            {
                alembicPlayer.CurrentTime += Time.deltaTime * speed;
            }
            yield return null;
        }

        foreach (Material mat in allMaterials)
        {
            if (mat.HasProperty("Fade"))
            {
                mat.SetFloat("Fade", 0f);
            }
        }
        finished = true;
    }
}

#endif