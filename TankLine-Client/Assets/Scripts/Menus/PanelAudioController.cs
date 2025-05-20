using UnityEngine;

public class PanelAudioController : MonoBehaviour
{
    public AudioSource audioSource;

    private void OnEnable()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
