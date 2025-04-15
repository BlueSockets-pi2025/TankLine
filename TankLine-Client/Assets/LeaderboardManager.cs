using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using Newtonsoft.Json;
using System;

public class LeaderboardManager : MonoBehaviour
{
    public string apiUrl = "https://185.155.93.105:17008/api/PlayedGames/by-user/";

    public TextMeshProUGUI map;
    public TextMeshProUGUI victories;
    public TextMeshProUGUI username;
    public TextMeshProUGUI ratio;
    public TextMeshProUGUI tanks_destroyed;
    public TextMeshProUGUI nb_games_played;
    public TextMeshProUGUI victory_or_defeat;
    public TextMeshProUGUI rank;
    public TextMeshProUGUI date;

    [System.Serializable]
    public class PlayedGameStats
    {
        public string username;
        public bool gameWon;
        public int tanksDestroyed;
        public int totalScore;
        public int playerRank;
        public string mapPlayed;
        public DateTime gameDate;

        public int totalGames;
        public int totalVictories;
    }

    void Start()
    {
        StartCoroutine(FetchUserStats(usernameToFetch));
    }


    IEnumerator FetchUserStats(string username)
{
    string fullUrl = apiUrl + username;
    UnityWebRequest request = UnityWebRequest.Get(fullUrl);
    request.SetRequestHeader("Content-Type", "application/json");

    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("Error: " + request.error);
    }
    else
    {
        string json = request.downloadHandler.text;

        PlayedGameStats stats = JsonConvert.DeserializeObject<PlayedGameStats>(json);

        if (stats != null)
        {
            float victoryRatio = stats.totalGames > 0
                ? (float)stats.totalVictories / stats.totalGames
                : 0f;

            this.username.text = stats.username;
            nb_games_played.text = stats.totalGames.ToString();
            victories.text = stats.totalVictories.ToString();
            ratio.text = victoryRatio.ToString("P1");
            tanks_destroyed.text = stats.tanksDestroyed.ToString();
            map.text = stats.mapPlayed;
            rank.text = stats.playerRank.ToString();
            victory_or_defeat.text = stats.gameWon ? "Victory" : "Defeat";
            date.text = stats.gameDate.ToString("dd/MM/yyyy HH:mm");
        }
        else
        {
            Debug.LogWarning("Invalid response from server.");
        }
    }
}

}
