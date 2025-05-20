using UnityEngine;

public enum GameState
{
    Connection, Menu, WaitingRoom, Playing, Victory, Lose, Score
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState CurrentStat { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        UpdateGameState(GameState.Connection);
        Debug.Log("Start GameManager");

        // Screen.orientation = ScreenOrientation.LandscapeLeft;
#if UNITY_ANDROID
		Screen.SetResolution(1280, 720, true);
		Application.targetFrameRate = 60;
#endif
    }

    //Update when the GameState changes
    public void UpdateGameState(GameState newState)
    {
        if (CurrentStat != newState)
        {
            CurrentStat = newState;
            SoundManager.Instance?.PlayMusic(newState);
            Debug.Log("Change State " + newState);

            switch (newState)
            {
                case GameState.Connection:
                    break;
                case GameState.Menu:
                    break;
                case GameState.WaitingRoom:
                    break;
                case GameState.Playing:
                    break;
                case GameState.Victory:
                    break;
                case GameState.Lose:
                    break;
                case GameState.Score:
                    break;
                default:
                    Debug.Log("ERROR: Unknown game state: " + newState);
                    break;
            }

        }
    }
}


// GameManager.Instance?.UpdateGameState(newState);

