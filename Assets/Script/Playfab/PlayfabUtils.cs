using UnityEngine;
using UnityEngine.UI;

public class PlayfabUtils
{
    public const string TITLE_ID = "BD903";
    public const string ENTITY_TYPE = "title_player_account";
    public const string MATCHMAKING_NAME = "MatchQueue";
    public const int MATCHMAKING_TIMEOUT = 120; // in seconds
    public const int MATCHMAKING_POLL_INTERVAL = 6; // in seconds

    static public void OnError(Text feedbackText, string error)
    {
        feedbackText.color = new Color(1, 0.75f, 0.75f, 1);
        feedbackText.text = error;
    }

    static public void OnSuccess(Text feedbackText, string success)
    {
        feedbackText.color = new Color(0.75f, 1, 0.75f, 1);
        feedbackText.text = success;
    }
}
