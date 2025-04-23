using UnityEngine;
using System.Collections.Generic;
using System.Collections; 
using UnityEngine.Formats.Alembic.Importer;


public class test_pull : MonoBehaviour
{
    private List<Material> allMaterials = new List<Material>();
    private AlembicStreamPlayer alembicPlayer;
    public float speed = 0.5f;
    private float timer = 0f;
    private Renderer rend;
    private bool finished;

    void Start()
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
                    mat.SetFloat("Fade", 0f);
                }
            }
        
        }

        alembicPlayer = GetComponent<AlembicStreamPlayer>();
        alembicPlayer.CurrentTime = 0;
        alembicPlayer.enabled = false;

        finished = false;

        StartCoroutine(PreloadAlembic());
        play();
    }

    private IEnumerator PreloadAlembic()
    {
        alembicPlayer.enabled = true;
        alembicPlayer.CurrentTime = alembicPlayer.Duration;
        yield return null; // attend une frame pour forcer le chargement
        alembicPlayer.enabled = false;
        alembicPlayer.CurrentTime = 0;
    }

    public void play() {
        alembicPlayer.enabled = true;
        foreach (Material mat in allMaterials)
        {
            if (mat.HasProperty("Fade"))
            {
                mat.SetFloat("Fade", 0f);
            }
        }
        finished = false;
        StartCoroutine(PerformAnimation());
        // alembicPlayer.CurrentTime = 1f;
        // finished = true;
    }

    protected void Update() {
        if (finished)
        {
            alembicPlayer.enabled = false;
            finished = false;
            alembicPlayer.CurrentTime = 0f;
            
            foreach (Material mat in allMaterials)
            {
                if (mat.HasProperty("Fade"))
                {
                    mat.SetFloat("Fade", 1f);
                }
            }

            play();
        }
    }

    private IEnumerator PerformAnimation()
    {
        while(alembicPlayer.CurrentTime < (alembicPlayer.Duration / speed))
        {
            if (alembicPlayer != null)
            {
                alembicPlayer.CurrentTime += Time.deltaTime*speed;
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


