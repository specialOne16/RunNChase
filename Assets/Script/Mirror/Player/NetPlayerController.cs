using UnityEngine;
using Mirror;
using static PlayerData;

public class NetPlayerController : NetworkBehaviour
{
    [Header("Server and Client Side")]
    [HideInInspector] [SyncVar(hook = nameof(Setup))] public int playerNumber;
    [SyncVar] public bool isAlive;
    [SyncVar(hook = nameof(SetPlayerTag))] public string playerTag;
    public NetMessageSystem messageSystem;

    [Header("Server Side")]
    [HideInInspector] public int score = 0;

    [Header("Client Side")]
    [HideInInspector] public string coloredName;
    [HideInInspector] public string coloredStatus;
    [HideInInspector] public Vector3 spawnPosition;
    [HideInInspector] public Quaternion spawnRotation;
    [HideInInspector] [SyncVar(hook = nameof(SetControl))] public bool controlEnabled = false;

    [Header("Boy Girl Configuration")]
    public Sprite boySprite;
    public Sprite girlSprite;
    public RuntimeAnimatorController boyAnimator;
    public RuntimeAnimatorController girlAnimator;

    private NetPlayerMovement movement;
    private PlayfabLogin loginManager;
    private PlayfabPlayer playerManager;

    [ClientCallback]
    private void Awake()
    {
        var playfabManager = GameObject.Find("PlayfabManager");
        if (playfabManager != null)
        {
            loginManager = playfabManager.GetComponent<PlayfabLogin>();
            playerManager = playfabManager.GetComponent<PlayfabPlayer>();
        }
    }

    private void Start()
    {
        messageSystem = GameObject.Find("MessageSystem").GetComponent<NetMessageSystem>();
    }

    public override void OnStartClient()
    {
        UpdatePlayerData(Color.black, "Waiting for other player...");
    }

    [Command]
    public void CmdUpdatePlayerData(string name, string status)
    {
        coloredName = name;
        coloredStatus = status;
        messageSystem.RpcUpdatePlayerStatus(playerNumber, name, status);
    }

    [ClientRpc]
    public void RpcSetSpawnPoint(Vector3 position, Quaternion rotation)
    {
        spawnPosition = position;
        spawnRotation = rotation;
    }
    
    public void SetPlayerTag(string oldPlayerTag, string newPlayerTag)
    {
        gameObject.tag = newPlayerTag;

        Color textColor = newPlayerTag == "Chaser" ? Color.red : Color.black;
        UpdatePlayerData(textColor, newPlayerTag);
    }

    [Client]
    public void UpdatePlayerData(Color textColor, string status)
    {
        if (!hasAuthority) return;

        var name = "Player " + playerNumber;
        if (loginManager != null)
        {
            name = loginManager.playerData.accountInfo.name;
        }
        coloredName = "<color=#" + ColorUtility.ToHtmlStringRGB(textColor) + ">" + name + "</color>";
        coloredStatus = "<color=#" + ColorUtility.ToHtmlStringRGB(textColor) + ">" + status + "</color>";
        CmdUpdatePlayerData(coloredName, coloredStatus);
    }

    [Client]
    public void Setup(int oldPlayerNumber, int newPlayerNumber)
    {
        movement = GetComponent<NetPlayerMovement>();

        movement.playerNumber = newPlayerNumber;

        if (playerNumber == 1)
        {
            GetComponent<SpriteRenderer>().sprite = boySprite;
            GetComponent<Animator>().runtimeAnimatorController = boyAnimator;
        } else
        {
            GetComponent<SpriteRenderer>().sprite = girlSprite;
            GetComponent<Animator>().runtimeAnimatorController = girlAnimator;
        }

        movement.enabled = false;
    }

    // Used during the phases of the game where the player should or shouldn't be able to control their tank.
    [Client]
    public void SetControl(bool oldControl, bool newControl)
    {
        movement.enabled = newControl;
    }

    [TargetRpc]
    public void TargetReceiveRewards(NetworkConnection target, string rawPlayerData)
    {
        Debug.Log($"Receive Reward: {rawPlayerData}");
        var newPlayerData = PlayerData.FromJson(rawPlayerData);
        var myPowerUps = loginManager != null ? loginManager.playerData.powerUps : new PlayerData().powerUps;

        myPowerUps.sprintTicket += newPlayerData.powerUps.sprintTicket;
        myPowerUps.marathonTicket += newPlayerData.powerUps.marathonTicket;
        myPowerUps.foodCoupon += newPlayerData.powerUps.foodCoupon;
        myPowerUps.milkCoupon += newPlayerData.powerUps.milkCoupon;
        myPowerUps.exchangeProgram += newPlayerData.powerUps.exchangeProgram;

        if (loginManager != null)
        {
            loginManager.playerData.stats.rankPoints += newPlayerData.stats.rankPoints;
            loginManager.playerData.powerUps = myPowerUps;
            playerManager.SendPlayerData(loginManager.playerData);
        }
    }

    // Used at the start of each round to put the tank into it's default state.
    [ClientRpc]
    public void RpcReset()
    {
        gameObject.transform.position = spawnPosition;
        gameObject.transform.rotation = spawnRotation;

        isAlive = true;

        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasAuthority) return;

        if (gameObject.CompareTag("Runner"))
        {
            if (collision.collider.CompareTag("Chaser"))
            {
                CmdDead();
            }
        }
    }

    [Command]
    private void CmdDead()
    {
        isAlive = false;
        RpcDead();
    }

    [ClientRpc]
    private void RpcDead()
    {
        isAlive = false;
        Debug.Log($"Player {playerNumber} catches!");
    }
}
