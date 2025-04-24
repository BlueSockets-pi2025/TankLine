using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic; 

public class MainMenuSwapper : MonoBehaviour
{
    public static MainMenuSwapper Instance { get; private set; }
    private AuthController authController;

    public GameObject _canvas;
    private Transform Canvas;
    public GameObject ErrorPopup, MessagePopup;

    public GameObject CurrentPage;

    public GameObject EmailErrorText; 
    public GameObject PasswordErrorText; 
    public GameObject ConfirmationErrorText; 

    public GameObject PasswordErrorTextReset; 
    public GameObject ConfirmationErrorTextReset; 

    public GameObject top1, top2, top3, top4; 

    // Testing : 
    public TMP_InputField gamesPlayedInputField; 
    public TMP_InputField highestScoreInputField; 

    void Awake()
    {
        // ensure there is only one instance of this script
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Canvas = _canvas.transform;
    }

    private void Start()
    {
        authController = GetComponent<AuthController>();
        if (authController == null)
        {
            Debug.LogError("AuthController not found.");
            return;
        }
        //AutoLogin();

        // load first page
        // OpenPage("PagePrincipale"); 
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "ConnectionMenu")
        {
            OpenPage("PagePrincipale"); 
        }
        else if (currentScene == "MainMenu")
        {
            OpenPage("MainMenu");
        }
        

        //OpenPagePrincipal();
    }

    private void AutoLogin()
    {
        StartCoroutine(AutoLoginCoroutine());
    }

    /// <summary>
    /// Coroutine function to log in
    /// </summary>
    private IEnumerator AutoLoginCoroutine()
    {
        Debug.Log("Attempting auto-login...");

        // Request requiring an access token to check if the user is already logged in and set the current user 
        yield return authController.User();
        if (authController.IsRequestSuccessful)
        {
            Debug.Log("Auto-login successful. Skipping login page.");
            OpenPage("MainMenu"); // Go directly to the main page
        }
        else
        {
            Debug.Log("Auto-login failed. Redirecting to login page.");
            OpenPage("PagePrincipale"); // Connection page 
        }
    }

    /// <summary>
    /// Open the page "pageName" in the canvas, and close every other page </br>
    /// The page must be a direct child of the main canvas
    /// </summary>
    /// <param name="pageName">The name of the page</param>
    public void OpenPage(string pageName)
    {

        // disable old page
        CurrentPage.SetActive(false);

        // find new page and enable it
        CurrentPage = Canvas.Find(pageName).gameObject;

        if (CurrentPage == null)
        {
            Debug.LogError($"Page not found : {pageName}");
            return;
        }

        CurrentPage.SetActive(true);

         if (pageName == "Score")
        {
            Debug.Log("Opening Score page. Updating leaderboard...");
            UpdateLeaderboard();
        }

        // if switch to mainMenu or play, load stats & name
        if (pageName == "MainMenu" || pageName == "Play")
        {
            StartCoroutine(UpdateStatisticsCoroutine(CurrentPage.transform.Find("Badge")));
        }
    }

    public void OpenErr(string message)
    {
        ErrorPopup.SetActive(true);
        ErrorPopup.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = message; // change popup main text
    }
    public void CloseErr()
    {
        ErrorPopup.SetActive(false);
    }

    public void OpenMessage(string message)
    {
        MessagePopup.SetActive(true);
        MessagePopup.transform.Find("Text").gameObject.GetComponent<TMP_Text>().text = message; // change popup main text
    }
    public void CloseMessage()
    {
        MessagePopup.SetActive(false);
    }

    public void LogoutUser()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }
        StartCoroutine(LogoutCoroutine());
    }

    private IEnumerator LogoutCoroutine()
    {
        yield return authController.Logout();

        if (authController.IsRequestSuccessful)
        {
            OpenMessage("You have been logged out successfully.");
        }
        else
        {
            OpenErr($"Logout failed. Please try again: \n {authController.ErrorResponse}");
        }
    }

    public void HandleSessionExpired()
    {
        Debug.Log("Redirecting to login page...");
        OpenPage("PagePrincipale"); // Redirects to login page
    }

    private IEnumerator UpdateStatisticsCoroutine(Transform badge)
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            yield break;
        }

        yield return authController.UserStatistics();

        if (authController.IsRequestSuccessful && authController.CurrentUserStatistics != null)
        {
            if (badge != null)
            {
                badge.Find("GamePlayed").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.gamesPlayed.ToString();
                badge.Find("HighScore").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.highestScore.ToString();
                badge.Find("Rank").GetComponent<TMP_Text>().text = authController.CurrentUserStatistics.ranking.ToString();
                badge.Find("UserName").GetComponent<TMP_Text>().text = authController.CurrentUser.username;

            }
            else
            {
                Debug.LogWarning("Badge undefined");
            }
        }
        else
        {
            OpenErr($"Failed to retrieve user statistics: \n {authController.ErrorResponse}");
        }
    }

    public void SetPlayerStatistics(int gamesPlayed, int highestScore)
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }

        StartCoroutine(authController.UpdateUserStatistics(
            gamesPlayed,
            highestScore,
            onSuccess: () =>
            {
                Debug.Log("Player statistics updated successfully.");
                OpenMessage("Statistics updated successfully.");
            },
            onError: (errorMessage) =>
            {
                Debug.LogError($"Failed to update player statistics: {errorMessage}");
                OpenErr($"Failed to update statistics: {errorMessage}");
            }
        ));
    }

    public void OnUpdateStatisticsButtonClick()
    {
        int gamesPlayed;
        int highestScore;

        if (!int.TryParse(gamesPlayedInputField.text, out gamesPlayed))
        {
            Debug.LogError("Invalid input for Games Played.");
            return;
        }

        if (!int.TryParse(highestScoreInputField.text, out highestScore))
        {
            Debug.LogError("Invalid input for Highest Score.");
            return;
        }
        SetPlayerStatistics(gamesPlayed, highestScore);
    }

    public void UpdateLeaderboard()
    {
        if (authController == null)
        {
            Debug.LogError("AuthController is not initialized.");
            return;
        }

        StartCoroutine(authController.GetLeaderboard(
            onSuccess: (leaderboardEntries) =>
            {
                Debug.Log("Leaderboard data fetched successfully.");
                UpdateLeaderboardUI(leaderboardEntries);
            },
            onError: (errorMessage) =>
            {
                Debug.LogError($"Failed to fetch leaderboard: {errorMessage}");
                OpenErr($"Failed to fetch leaderboard: {errorMessage}");
            }
        ));
    }

    private void UpdateLeaderboardUI(List<LeaderboardEntry> leaderboardEntries)
    {
        // Update GameObjects for the first 4
        UpdateLeaderboardEntry(top1, leaderboardEntries.Count > 0 ? leaderboardEntries[0] : null);
        UpdateLeaderboardEntry(top2, leaderboardEntries.Count > 1 ? leaderboardEntries[1] : null);
        UpdateLeaderboardEntry(top3, leaderboardEntries.Count > 2 ? leaderboardEntries[2] : null);
        UpdateLeaderboardEntry(top4, leaderboardEntries.Count > 3 ? leaderboardEntries[3] : null);
    }

    private void UpdateLeaderboardEntry(GameObject entryObject, LeaderboardEntry entry)
    {
        if (entryObject == null) return;

        // Retrieve sub-GameObjects for Rank, Name and Score
        TMP_Text rankText = entryObject.transform.Find("Rank")?.GetComponent<TMP_Text>();
        TMP_Text nameText = entryObject.transform.Find("Name")?.GetComponent<TMP_Text>();
        TMP_Text scoreText = entryObject.transform.Find("Score")?.GetComponent<TMP_Text>();

        if (entry != null)
        {
            // Update text fields
            if (rankText != null) rankText.text = $"{entry.ranking}";
            if (nameText != null) nameText.text = entry.username;
            if (scoreText != null) scoreText.text = $"{entry.highestScore}";

            entryObject.SetActive(true); // Display entry
        }
        else
        {
            // Hide fields if no data available
            if (rankText != null) rankText.text = "";
            if (nameText != null) nameText.text = "";
            if (scoreText != null) scoreText.text = "";

            entryObject.SetActive(false); // Hide entry
        }
    }

    public void ConnectToRandomWaitingRoom() {
        ConnectToWaitingRoom();
    }

    public void ConnectToWaitingRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("WaitingRoom");
    }

    public void ConnectionScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ConnectionMenu");
    }
    public void MainMenuScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public static void ShowTooltip(GameObject tooltip) {
        if (tooltip != null) {
            tooltip.SetActive(true); // Affiche le tooltip
        } else {
            Debug.LogWarning("Tooltip is not assigned in the inspector.");
        }
    }

    public static void HideTooltip(GameObject tooltip) {
        if (tooltip != null) {
            tooltip.SetActive(false); // Cache le tooltip
        } else {
            Debug.LogWarning("Tooltip is not assigned in the inspector.");
        }
    }
}
