using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayfabLeaderboard : MonoBehaviour
{
    [Header("Text Feedback Fields")]
    public Text feedbackText;
    public Text usernameText;

    [Header("LeaderboardItems")]
    public GameObject tableItemPrefab;
    public Transform tableTransform;

    private PlayfabLogin loginManager;

    public void OnPageActive()
    {
        loginManager = gameObject.GetComponent<PlayfabLogin>();
        feedbackText.text = "";
        usernameText.text = loginManager.getDisplayName();
        if (loginManager.isLoggedIn())
        {
            GetLeaderboard();
        } else
        {
            PlayfabUtils.OnError(feedbackText, "You must login first to see the leaderboard!");
        }
    }

    public void SendWinStatistic(int newWin)
    {
        if (!loginManager.isLoggedIn())
        {
            Debug.LogError("Win Statistic failed to sent: Not logged in.");
            return;
        }
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "MostWin",
                    Value = newWin
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnWinSent, OnError);
    }

    private void OnWinSent(UpdatePlayerStatisticsResult res)
    {
        Debug.Log("Send win statistic success!");
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "MostWin",
            MaxResultsCount = 6
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(request, OnLeaderboardGet, OnError);
    }

    public void OnLeaderboardGet(GetLeaderboardAroundPlayerResult res)
    {
        foreach (Transform item in tableTransform)
        {
            Destroy(item.gameObject);
        }
        foreach (var item in res.Leaderboard)
        {
            GameObject newItem = Instantiate(tableItemPrefab, tableTransform);
            Text[] texts = newItem.GetComponentsInChildren<Text>();
            texts[0].text = (item.Position + 1).ToString();
            texts[1].text = item.DisplayName;
            texts[2].text = item.StatValue.ToString();
            if (item.PlayFabId == loginManager.playerData.accountInfo.playfabId)
            {
                texts[0].color = Color.yellow;
                texts[1].color = Color.yellow;
                texts[2].color = Color.yellow;
            }
        }
        PlayfabUtils.OnSuccess(feedbackText, "Leaderboard Updated!");
    }

    private void OnError(PlayFabError error)
    {
        PlayfabUtils.OnError(feedbackText, error.ErrorMessage);
    }
}
