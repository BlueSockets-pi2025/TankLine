using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUiManager
{
    private readonly GameObject canvas;
    private readonly GameObject playerCount;
    private readonly GameObject startButton;
    private readonly GameObject respawnCountDown;
    private readonly GameObject playerListDiv;
    private readonly GameObject gameOverPanel;
    private readonly GameObject skinGrid;
    private readonly GameObject playerListDivWrap;
    private readonly GameObject lifePanel;
    private readonly GameObject bulletPanel;

    private bool isSettingsPanelOpen = false;
    private bool isSkinPanelOpen = false;
    private bool isInGame;

    public InGameUiManager(GameObject _canvas, bool _isInGame)
    {
        canvas = _canvas;
        isInGame = _isInGame;
        if (canvas == null)
        {
            Debug.LogError("Critical error, canvas is null");
            return;
        }

        // search gameobject
        playerListDivWrap = canvas.transform.Find("WrapPlayer").gameObject;
        playerListDiv = playerListDivWrap.transform.Find("PlayersCanvas").gameObject;

        // add onclick function to buttons
        canvas.transform.Find("GearButton").GetComponent<Button>().onClick.AddListener(this.ClickSettingsPanel);
        canvas.transform.Find("SettingsPanel").Find("ExitButton").GetComponent<Button>().onClick.AddListener(this.ExitToMenu);

#if UNITY_ANDROID
        canvas.transform.Find("SettingsPanel").Find("OptionKeyboard").gameObject.SetActive(false);
        canvas.transform.Find("SettingsPanel").Find("OptionGeneral/keyboard").gameObject.SetActive(false);
        canvas.transform.Find("SettingsPanel").Find("OptionMouse/keyboard").gameObject.SetActive(false);
        canvas.transform.Find("SettingsPanel").Find("OptionMouse").gameObject.SetActive(false);
        canvas.transform.Find("SettingsPanel").Find("OptionGeneral/mouse").gameObject.SetActive(false);
        canvas.transform.Find("SettingsPanel").Find("OptionKeyboard/mouse").gameObject.SetActive(false);
#endif

        // waiting room ui
        if (!isInGame)
        {
            startButton = canvas.transform.Find("StartButton").gameObject;
            playerCount = canvas.transform.Find("PlayerCount").gameObject;
            skinGrid = canvas.transform.Find("SkinPanel").Find("SkinGrid").gameObject;
            canvas.transform.Find("SkinPanel").gameObject.SetActive(isSkinPanelOpen);

            canvas.transform.Find("StartButton").GetComponent<Button>().onClick.AddListener(MonoBehaviour.FindFirstObjectByType<LobbyManager>().ClickedStartGame);
            canvas.transform.Find("SkinPickerButton").GetComponent<Button>().onClick.AddListener(MonoBehaviour.FindAnyObjectByType<LobbyManager>().GetAvailableSkins);
            canvas.transform.Find("SkinPickerButton").GetComponent<Button>().onClick.AddListener(this.ClickSkinsPanel);
            canvas.transform.Find("SkinPanel").Find("CloseBtn").GetComponent<Button>().onClick.AddListener(this.ClickSkinsPanel);

#if UNITY_ANDROID
            RectTransform startUI = startButton.GetComponent<RectTransform>();
            startUI.anchorMin = new Vector2(0.5f, 0f);
            startUI.anchorMax = new Vector2(0.5f, 0f);
            startUI.pivot = new Vector2(0.5f, 0f);
            startUI.anchoredPosition = new Vector2(0f, 100f);
#endif

            // disable start button at the beggining
            DisableStartButton();
        }

        // in game ui
        if (isInGame)
        {
            respawnCountDown = canvas.transform.Find("RespawnCountdown").gameObject;
            gameOverPanel = canvas.transform.Find("GameOverPanel").gameObject;
            lifePanel = canvas.transform.Find("PlayerLife").gameObject;
            bulletPanel = canvas.transform.Find("PlayerBullet").gameObject;

#if UNITY_ANDROID
            RectTransform bulletUI = bulletPanel.GetComponent<RectTransform>();
            bulletUI.anchorMin = new Vector2(0.5f, 0f);
            bulletUI.anchorMax = new Vector2(0.5f, 0f);
            bulletUI.pivot = new Vector2(0.5f, 0f);
            bulletUI.anchoredPosition = new Vector2(0f, 100f);
#endif

            ResetGameOverPanel();

            // bind onclick
            gameOverPanel.transform.Find("ExitBtn").GetComponent<Button>().onClick.AddListener(this.ExitToMenu);
            gameOverPanel.transform.Find("SpectateGameBtn").GetComponent<Button>().onClick.AddListener(() => { gameOverPanel.SetActive(false); });
            gameOverPanel.transform.Find("ReplayBtn").GetComponent<Button>().onClick.AddListener(MonoBehaviour.FindAnyObjectByType<LobbyManager>().SendReadyForReplay);

            gameOverPanel.SetActive(false); // hide panel
        }
    }


    public void ClickSettingsPanel()
    {
        isSettingsPanelOpen = !isSettingsPanelOpen;
        canvas.transform.Find("SettingsPanel").gameObject.SetActive(isSettingsPanelOpen);
    }

    public void ClickSkinsPanel()
    {
        isSkinPanelOpen = !isSkinPanelOpen;
        canvas.transform.Find("SkinPanel").gameObject.SetActive(isSkinPanelOpen);
    }

    public void ExitToMenu()
    {
        // Disconnect, then return to menu
        MonoBehaviour.FindAnyObjectByType<LobbyManager>().DisconnectClient(MonoBehaviour.FindAnyObjectByType<LobbyManager>().ClientManager.Connection);
        MonoBehaviour.Destroy(MonoBehaviour.FindFirstObjectByType<NetworkManager>().gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void DisableStartButton()
    {
        startButton.GetComponent<Button>().interactable = false;
        startButton.transform.Find("startText").GetComponent<TMP_Text>().color = new(.5f, .5f, .5f, .5f);
    }

    private void EnableStartButton()
    {
        startButton.GetComponent<Button>().interactable = true;
        startButton.transform.Find("startText").GetComponent<TMP_Text>().color = new(0f, 0f, 0f, 1f);
    }

    /// <summary>
    /// Update the whole UI of the waiting room (player names, player count & start button)
    /// </summary>
    /// <param name="clientPlayerList">The list of the player names</param>
    /// <param name="playerEntryPrefab">The prefab of a single player name entry</param>
    /// <param name="minimumPlayerToStart">The minimum number of player required to be able to start a game</param>
    public void UpdateUI(List<PlayerData> clientPlayerList, GameObject playerEntryPrefab, int minimumPlayerToStart)
    {
        // clear the name list
        foreach (Transform child in playerListDiv.transform)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }

        // put every entry in the UI list
        foreach (PlayerData player in clientPlayerList)
        {
            GameObject newEntry = MonoBehaviour.Instantiate(playerEntryPrefab, playerListDiv.transform);
            newEntry.name = player.name;
            newEntry.GetComponent<TMP_Text>().text = player.name;
        }

        // change player count on UI
        if (!isInGame)
            playerCount.GetComponent<TMP_Text>().text = clientPlayerList.Count.ToString() + "/6";

        // enable/disable start button if enough people
        if (!isInGame)
        {
            if (clientPlayerList.Count >= minimumPlayerToStart)
                EnableStartButton();
            else
                DisableStartButton();
        }
    }

    public System.Collections.IEnumerator RespawnCountdownCoroutine(int timeBeforeRespawn)
    {
        respawnCountDown.SetActive(true);

        TMP_Text txt = respawnCountDown.GetComponent<TMP_Text>();
        for (int i = timeBeforeRespawn; i > 0; i--)
        {
            txt.text = $"Respawning in {i} seconds";
            yield return new WaitForSeconds(1);
        }

        respawnCountDown.SetActive(false);
    }

    public void SetLifeUI(int newlife)
    {
        // If there is less life than before
        for (int i = 3; i > newlife; i--)
        {
            lifePanel.transform.Find($"heart_{i}").gameObject.SetActive(false);
        }
        // If there is more life than before
        for (int i = 1; i <= newlife; i++)
        {
            lifePanel.transform.Find($"heart_{i}").gameObject.SetActive(true);
        }
    }

    public void SetBulletUI(int newShotbullet, int bulletmax)
    {
        // If there is less bullet than before
        for (int i = bulletmax; i > (bulletmax - newShotbullet); i--)
        {
            bulletPanel.transform.Find($"bullet_{i}").gameObject.SetActive(false);
        }
        // If there is more bullet than before
        for (int i = 1; i <= (bulletmax - newShotbullet); i++)
        {
            bulletPanel.transform.Find($"bullet_{i}").gameObject.SetActive(true);
        }
    }

    public void ShowDefeatPanel()
    {
        gameOverPanel.SetActive(true);
        ResetGameOverPanel();

        gameOverPanel.transform.Find("Defeat").gameObject.SetActive(true);
        gameOverPanel.transform.Find("SpectateGameBtn").gameObject.SetActive(true);
    }

    public void ShowEndGamePanel(string winnerName, bool isWinner)
    {
        gameOverPanel.SetActive(true);
        ResetGameOverPanel();

        if (isWinner)
            gameOverPanel.transform.Find("Victory").gameObject.SetActive(true);
        else
        {
            gameOverPanel.transform.Find("GameOver").gameObject.SetActive(true);
            gameOverPanel.transform.Find("GameOver").Find("WinnerName").GetComponent<TMP_Text>().text = $"Winner: {winnerName}";
        }

        gameOverPanel.transform.Find("ReplayBtn").gameObject.SetActive(true);
    }

    public void UpdateReadyForReplay(int nbPlayer, int nbReady)
    {
        GameObject ready = gameOverPanel.transform.Find("playerReady").gameObject;

        ready.SetActive(true);
        ready.GetComponent<TMP_Text>().text = $"{nbReady}/{nbPlayer}";
    }

    public System.Collections.IEnumerator StartAutoReplayCountdown(int nbSeconds)
    {
        gameOverPanel.transform.Find("autoReplay").gameObject.SetActive(true);
        TMP_Text countdown = gameOverPanel.transform.Find("autoReplay").GetComponent<TMP_Text>();

        for (int i = nbSeconds; i > 0; i--)
        {
            countdown.text = $"auto-replay in {i} seconds";
            yield return new WaitForSeconds(1);
        }
    }

    private void ResetGameOverPanel()
    {
        gameOverPanel.transform.Find("Defeat").gameObject.SetActive(false);
        gameOverPanel.transform.Find("Victory").gameObject.SetActive(false);
        gameOverPanel.transform.Find("GameOver").gameObject.SetActive(false);
        gameOverPanel.transform.Find("SpectateGameBtn").gameObject.SetActive(false);
        gameOverPanel.transform.Find("ReplayBtn").gameObject.SetActive(false);
        gameOverPanel.transform.Find("playerReady").gameObject.SetActive(false);
        gameOverPanel.transform.Find("autoReplay").gameObject.SetActive(false);
    }

    public void CreateSkinsPanel(List<Material> skins, GameObject skinEntryPrefab)
    {

        // clear the skin picker just in case
        foreach (Transform child in skinGrid.transform)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }

        int i = 0;
        // create a entry for every skin
        foreach (Material skin in skins)
        {
            GameObject newEntry = MonoBehaviour.Instantiate(skinEntryPrefab, skinGrid.transform);
            newEntry.GetComponent<RawImage>().color = skin.color;
            newEntry.GetComponent<Button>().onClick.AddListener(() => { MonoBehaviour.FindAnyObjectByType<LobbyManager>().PickSkin(skins.IndexOf(skin)); });

            i++;
        }
    }

    public void UpdateSkinsPanel(List<int> availableSkins)
    {

        // update which skin is clickable or not
        for (int i = 0; i < skinGrid.transform.childCount; i++)
        {
            GameObject skinEntry = skinGrid.transform.GetChild(i).gameObject;

            // check if skin is available
            skinEntry.GetComponent<Button>().interactable = availableSkins.Contains(i);
        }
    }

    public void UpdateAllInput()
    {
        foreach (Tank_Player tank in GameObject.FindObjectsOfType<Tank_Player>())
        {
            tank.inpuuttt();
        }
        foreach (Tank_Player tank in GameObject.FindObjectsOfType<Tank_Player>())
        {
            tank.desactiveInput();
        }
        foreach (Tank_Player tank in GameObject.FindObjectsOfType<Tank_Player>())
        {
            tank.activeInput();
        }
    }
}