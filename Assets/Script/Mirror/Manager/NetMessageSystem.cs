using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetMessageSystem : NetworkBehaviour
{
    [Header("Player Texts")]
    public Text player1Name;
    public Text player1Status;
    public Text player2Name;
    public Text player2Status;

    [Header("Score Texts")]
    public Text player1Score;
    public Text player2Score;

    [Header("Timer Text")]
    public Text timer;

    [Header("Message Feedback")]
    public Text message;

    [ClientRpc]
    public void RpcUpdatePlayerStatus(int playerNumber, string name, string status)
    {
        if (playerNumber == 1)
        {
            player1Name.text = name;
            player1Status.text = status;
        } else if (playerNumber == 2)
        {
            player2Name.text = name;
            player2Status.text = status;
        }
    }

    [ClientRpc]
    public void RpcUpdateScore(int player1Score, int player2Score)
    {
        this.player1Score.text = player1Score.ToString();
        this.player2Score.text = player2Score.ToString();
    }

    [ClientRpc]
    public void RpcUpdateTimer(string time)
    {
        timer.text = time;
    }

    [ClientRpc]
    public void RpcBroadcastMessage(string message)
    {
        this.message.text = message;
    }
}
