using UnityEngine;
// using FishNet.Object;
// using FishNet.Managing;

public enum GameState
{
    Connection, Menu, WaitingRoom, Playing, Victory, Lose, Score //add ready?
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState CurrentStat { get; private set; }

    // public bool HostPlayer { get; set; }

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
        // CurrentStat = GameState.Menu;
        UpdateGameState(GameState.Connection);
        Debug.Log("Start GameManager");
    }

    //Update when the GameState changes
    public void UpdateGameState(GameState newState)
    {
        // State = newState;
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
                    // throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
                    Debug.Log("ERROR: Unknown game state: " + newState);
                    break;
            }

        }
    }
}


// GameManager.Instance?.UpdateGameState(newState);

