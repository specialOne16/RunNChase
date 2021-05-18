using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using PlayFab.Networking;
using static PlayerData;

public class NetGameManager : NetworkManager
{
    [Header("Client Side")]
    public NetMessageSystem messageSystem;

    [Header("Server Side")]
    public int MAX_PLAYERS = 2;
    public int ROUNDS_TO_WIN = 5;
    public float START_DELAY = 3f;
    public float END_DELAY = 3f;
    public float MATCH_DURATION = 10;

    [Header("Players Configuration")]
    [SerializeField] public Transform[] spawnPoints;
    [HideInInspector] public List<NetPlayerController> players;

    [Header("Playfab Multiplayer Configuration")]
    public UnityNetworkServer UNetServer;

    private int match = 0;
    private WaitForSeconds startWait;
    private WaitForSeconds endWait;
    private float timeRemaining;
    private bool timerIsRunning;
    private NetPlayerController roundWinner;
    private NetPlayerController gameWinner;

    [ClientCallback]
    private void Start()
    {
        // BG Sound play here
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        startWait = new WaitForSeconds(START_DELAY);
        endWait = new WaitForSeconds(END_DELAY);

        SpawnPlayer(conn);
        Debug.Log("Player Spawned!");

        if (numPlayers == MAX_PLAYERS)
        {
            messageSystem.RpcUpdateScore(0, 0);
            StartCoroutine(GameLoop());
        }
    }

    private void SpawnPlayer(NetworkConnection conn)
    {
        NetPlayerController player = Instantiate(playerPrefab, spawnPoints[numPlayers].position, spawnPoints[numPlayers].rotation).GetComponent<NetPlayerController>();
        
        NetworkServer.AddPlayerForConnection(conn, player.gameObject);
        player.RpcSetSpawnPoint(spawnPoints[numPlayers-1].position, spawnPoints[numPlayers-1].rotation);
        player.playerNumber = numPlayers;

        players.Add(player);
    }

    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());

        yield return StartCoroutine(RoundPlaying());

        yield return StartCoroutine(RoundEnding());

        if (gameWinner != null)
        {
            yield return StartCoroutine(GiveRewards());
            ServerChangeScene("MainMenu");
            Shutdown();
            yield return new WaitForSeconds(30f);
            Application.Quit();
            //SceneManager.LoadSceneAsync(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator RoundStarting()
    {
        ResetAllPlayers();
        DisablePlayerControl();

        match++;
        players[(match + 1) % 2].playerTag = "Chaser";
        players[match % 2].playerTag = "Runner";

        Debug.Log("Round starting...");

        timeRemaining = MATCH_DURATION;
        UpdateTimer();

        StartCoroutine(Countdown());

        yield return startWait;
    }

    private IEnumerator Countdown()
    {
        messageSystem.RpcBroadcastMessage("Round Starting in 3...");
        yield return new WaitForSeconds(1);

        messageSystem.RpcBroadcastMessage("Round Starting in 2...");
        yield return new WaitForSeconds(1);

        messageSystem.RpcBroadcastMessage("Round Starting in 1...");
        yield return new WaitForSeconds(1);

        messageSystem.RpcBroadcastMessage("START!");
        yield return new WaitForSeconds(1);

        messageSystem.RpcBroadcastMessage("");
    }

    private IEnumerator RoundPlaying()
    {
        EnablePlayerControl();
        timerIsRunning = true;
        Debug.Log("Round playing...");
        Debug.Log($"{!OnePlayerLeft()} && {!GameTimeout()}");

        while (!OnePlayerLeft() && !GameTimeout())
        {
            yield return null;
        }

        timerIsRunning = false;
    }

    private IEnumerator RoundEnding()
    {
        DisablePlayerControl();

        roundWinner = null;

        roundWinner = GetRoundWinner();

        if (roundWinner != null)
            roundWinner.score++;

        gameWinner = GetGameWinner();
        Debug.Log("Round ending...");

        messageSystem.RpcBroadcastMessage(EndMessage());
        messageSystem.RpcUpdateScore(players[0].score, players[1].score);

        yield return endWait;
    }

    private string EndMessage()
    {
        string message = "DRAW!";

        if (roundWinner != null)
            message = roundWinner.coloredName + " WINS THE ROUND!";

        message += "\n\n\n";

        for (int i = 0; i < players.Count; i++)
        {
            message += players[i].coloredName + ": " + players[i].score + " WINS\n";
        }

        if (gameWinner != null)
            message = gameWinner.coloredName + " WINS THE GAME!";

        return message;
    }

    private bool OnePlayerLeft()
    {
        int numPlayersLeft = 0;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].isAlive)
                numPlayersLeft++;
        }

        return numPlayersLeft <= 1;
    }

    private bool GameTimeout()
    {
        return !timerIsRunning;
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 1)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimer();
            }
            else
            {
                timerIsRunning = false;
            }
        }
    }

    private void UpdateTimer()
    {
        int minutes = (int) timeRemaining / 60;
        int seconds = (int) timeRemaining - 60 * minutes;
        string time = string.Format("{0:00}:{1:00}", minutes, seconds);
        messageSystem.RpcUpdateTimer(time);
    }

    private NetPlayerController GetRoundWinner()
    {
        if (timeRemaining <= 1)
        {
            return players.Find(player => player.playerTag == "Runner");
        }
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].isAlive)
                return players[i];
        }

        return null;
    }


    private NetPlayerController GetGameWinner()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].score == ROUNDS_TO_WIN)
                return players[i];
        }

        return null;
    }

    private IEnumerator GiveRewards()
    {
        for (int i = 0; i < players.Count; i++)
        {
            PowerUps powerUps;
            int rankPoints;
            if (players[i] == gameWinner)
            {
                powerUps = new PowerUps
                {
                    sprintTicket = Random.Range(10, 15),
                    marathonTicket = Random.Range(5, 10),
                    foodCoupon = Random.Range(4, 6),
                    milkCoupon = Random.Range(4, 6),
                    exchangeProgram = Random.Range(1, 2)
                };
                rankPoints = Random.Range(8, 12);
            }
            else
            {
                powerUps = new PowerUps
                {
                    sprintTicket = Random.Range(2, 5),
                    marathonTicket = Random.Range(1, 4),
                    foodCoupon = Random.Range(0, 2),
                    milkCoupon = Random.Range(0, 2),
                    exchangeProgram = 0
                };
                rankPoints = Random.Range(2, 6);
            }
            var newPlayerData = new PlayerData();
            newPlayerData.powerUps = powerUps;
            newPlayerData.stats.rankPoints = rankPoints;
            var jsonData = newPlayerData.ToJson();

            var playerConn = players[i].gameObject.GetComponent<NetworkIdentity>().connectionToClient;
            Debug.Log($"send data to player {i + 1} conn: {playerConn}");
            players[i].TargetReceiveRewards(playerConn, jsonData);

            var message = "Your got rewards!";
            message += "\nSprint Ticket +" + powerUps.sprintTicket.ToString();
            message += "     Marathon Ticket +" + powerUps.marathonTicket.ToString();
            message += "\nFood Coupon +" + powerUps.foodCoupon.ToString();
            message += "     Milk Coupon +" + powerUps.milkCoupon.ToString();
            message += "\nPlayer Exchange Program +" + powerUps.exchangeProgram.ToString();
            message += "\nRank Points +" + rankPoints.ToString();

            messageSystem.TargetMessage(playerConn, message);
        }
        Debug.Log("send rewards completed!");
        yield return new WaitForSeconds(5);
    }

    private void ResetAllPlayers()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].isAlive = true;
            players[i].RpcReset();
        }
    }

    private void EnablePlayerControl()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].controlEnabled = true;
        }
    }

    private void DisablePlayerControl()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].controlEnabled = false;
        }
    }

    private void OnGUI()
    {
        if (timerIsRunning)
        {
            GUI.TextArea(new Rect(Screen.width / 2 - 200, Screen.height/4, 400, 110), $"{Mathf.RoundToInt(timeRemaining)} \n{players[0].score} - {players[1].score}", new GUIStyle(GUI.skin.textArea));
        }
    }

    #region UnityNetworkServer
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        UNetServer.OnServerConnect(conn);
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        UNetServer.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        SceneManager.LoadSceneAsync(0);
    }

    #endregion
}
