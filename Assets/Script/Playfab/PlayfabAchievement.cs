using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayfabAchievement : MonoBehaviour
{
    [Header("Text Feedback Fields")]
    public Text feedbackText;
    public Text usernameText;

    [Header("LeaderboardItems")]
    public GameObject tableItemPrefab;
    public Transform tableTransform;

    private PlayfabLoginRegister loginManager;

    public void OnPageActive()
    {
        loginManager = gameObject.GetComponent<PlayfabLoginRegister>();
        feedbackText.text = "";
        usernameText.text = loginManager.getDisplayName();
        if (loginManager.isLoggedIn())
        {
            GetLeaderboard();
        } else
        {
            OnError("You must login first to see the leaderboard!");
        }
    }

    public void SendScore()
    {
        if (!loginManager.isLoggedIn())
        {
            OnError("Send score needs login first!");
            return;
        }
        var randomScore = Random.Range(0, 100);
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "HighScore",
                    Value = randomScore
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnScoreSent, OnError);
    }

    private void OnScoreSent(UpdatePlayerStatisticsResult res)
    {
        OnSuccess("Send score success!");
        Invoke(nameof(GetLeaderboard), .5f);
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "HighScore",
            StartPosition = 0,
            MaxResultsCount = 6
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    public void OnLeaderboardGet(GetLeaderboardResult res)
    {
        foreach (Transform item in tableTransform)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in res.Leaderboard)
        {
            GameObject newItem = Instantiate(tableItemPrefab, tableTransform);
            Text[] texts = newItem.GetComponentsInChildren<Text>();
            texts[0].text = item.Position.ToString();
            texts[1].text = item.DisplayName;
            texts[2].text = item.StatValue.ToString();
            if (item.DisplayName == loginManager.getDisplayName())
            {
                texts[0].color = Color.yellow;
                texts[1].color = Color.yellow;
                texts[2].color = Color.yellow;
            }
        }
        OnSuccess("Leaderboard Updated!");
    }

    private void OnError(PlayFabError error)
    {
        feedbackText.color = new Color(1, 0.75f, 0.75f, 1);
        feedbackText.text = error.ErrorMessage;
    }

    private void OnError(string error)
    {
        feedbackText.color = new Color(1, 0.75f, 0.75f, 1);
        feedbackText.text = error;
    }

    private void OnSuccess(string success)
    {
        feedbackText.color = new Color(0.75f, 1, 0.75f, 1);
        feedbackText.text = success;
    }
}
