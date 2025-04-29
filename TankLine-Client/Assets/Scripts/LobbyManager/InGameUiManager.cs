using System.Collections.Generic;
using FishNet.Managing;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUiManager
{
    private readonly GameObject canvas;
    private readonly GameObject playerListDiv;
    private readonly GameObject playerCount;
    private readonly GameObject startButton;
    private bool isSettingsPanelOpen = false;
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
        playerListDiv = canvas.transform.Find("PlayersCanvas").gameObject;

        // add onclick function to buttons
        canvas.transform.Find("GearButton").GetComponent<Button>().onClick.AddListener(this.ClickPanel);
        canvas.transform.Find("SettingsPanel").Find("ExitButton").GetComponent<Button>().onClick.AddListener(this.ExitToMenu);

        if (!isInGame) {
            startButton = canvas.transform.Find("StartButton").gameObject;
            playerCount = canvas.transform.Find("PlayerCount").gameObject;
            canvas.transform.Find("StartButton").GetComponent<Button>().onClick.AddListener(MonoBehaviour.FindFirstObjectByType<LobbyManager>().ClickedStartGame);

            // disable start button at the beggining
            DisableStartButton();
        }
    }


    public void ClickPanel()
    {
        isSettingsPanelOpen = !isSettingsPanelOpen;
        canvas.transform.Find("SettingsPanel").gameObject.SetActive(isSettingsPanelOpen);
    }

    public void ExitToMenu()
    {
        // Disconnect, then return to menu
        MonoBehaviour.FindFirstObjectByType<LobbyManager>().DisconnectClient(MonoBehaviour.FindFirstObjectByType<LobbyManager>().GetComponent<NetworkObject>().ClientManager.Connection);
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
    public void UpdateUI(List<string> clientPlayerList, GameObject playerEntryPrefab, int minimumPlayerToStart)
    {
        // clear the name list
        foreach (Transform child in playerListDiv.transform)
        {
            MonoBehaviour.Destroy(child.gameObject);
        }

        // put every entry in the UI list
        foreach (string name in clientPlayerList)
        {
            GameObject newEntry = MonoBehaviour.Instantiate(playerEntryPrefab, playerListDiv.transform);
            newEntry.name = name;
            newEntry.GetComponent<TMP_Text>().text = name;
        }

        // change player count on UI
        if (!isInGame)
            playerCount.GetComponent<TMP_Text>().text = clientPlayerList.Count.ToString() + "/6";

        // enable/disable start button if enough people
        if (!isInGame) {
            if (clientPlayerList.Count >= minimumPlayerToStart)
                EnableStartButton();
            else
                DisableStartButton();
        }
    }
}