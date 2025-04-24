using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Musics")]
    public AudioClip menuMusic;
    public AudioClip inGameMusic;

    [Header("SFX")]
    public AudioClip btnClicSfx;
    public AudioClip deathSfx;
    public AudioClip winnerSfx;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayMusic(GameState state)
    {
        AudioClip clipToPlay = null;
        Debug.Log("PlayMusic " + state);

        switch (state)
        {
            case GameState.Connection:
                break;
            case GameState.Menu:
                clipToPlay = menuMusic;
                break;
            case GameState.WaitingRoom:
                clipToPlay = menuMusic;
                break;
            case GameState.Playing:
                clipToPlay = inGameMusic;
                break;
            case GameState.Victory:
                break;
            case GameState.Lose:
                break;
            case GameState.Score:
                break;
        }

        if (clipToPlay != null && musicSource.clip != clipToPlay)
        {
            musicSource.clip = clipToPlay;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    //SoundManager.Instance.PlaySFX(pingClip);

}

