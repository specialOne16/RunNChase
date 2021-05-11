using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class NetPlayerController : NetworkBehaviour
{
    [Header("Server and Client Side")]
    [HideInInspector] [SyncVar(hook = nameof(Setup))] public int playerNumber;
    [SyncVar] public bool isAlive;
    [SyncVar(hook = nameof(SetPlayerTag))] public string playerTag;

    [Header("Server Side")]
    [HideInInspector] public int score = 0;

    [Header("Client Side")]
    public Text nameText;
    [HideInInspector] public Vector3 spawnPosition;
    [HideInInspector] public Quaternion spawnRotation;
    [HideInInspector] [SyncVar(hook = nameof(SetControl))] public bool controlEnabled = false;

    [Header("Boy Girl Configuration")]
    public Sprite boySprite;
    public Sprite girlSprite;
    public RuntimeAnimatorController boyAnimator;
    public RuntimeAnimatorController girlAnimator;

    private NetPlayerMovement movement;
    private PlayfabLoginRegister loginManager;

    [ClientRpc]
    public void RpcSetSpawnPoint(Vector3 position, Quaternion rotation)
    {
        spawnPosition = position;
        spawnRotation = rotation;
    }
    
    public void SetPlayerTag(string oldPlayerTag, string newPlayerTag)
    {
        gameObject.tag = newPlayerTag;
        Debug.Log($"Player {playerNumber} tag: {playerTag}");
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
