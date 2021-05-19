using UnityEngine;
using Mirror;

public class NetPlayerMovement : NetworkBehaviour
{
    [Header("Game Configuration")]
    [HideInInspector] public int playerNumber = 1;
    [HideInInspector] public float stamina = 100;

    [Header("Physics Configuration")]
    public float speed = 10;
    public float acceleration = 15;
    public float jumpForce = 5;
    public bool isGrounded = false;

    [Header("Player Slide")]
    public float slideForce = 2.5f;
    public float slideTime = 0.5f;
    private bool isSliding = false;
    private float timeCount;

    // Wall Slide
    [Header("Player Wall Slide")]
    public float wallSlideSpeed;
    [SerializeField] LayerMask wall;
    [SerializeField] Transform wallCheckPoint;
    [SerializeField] Vector2 wallCheckSize;

    private bool isTouchingWall;
    private bool isWallSlide;

    // Wall Jump
    [Header("Player Wall Jump")]
    public float wallJumpForce;
    public float wallJumpDirection = -1f;
    public float airMoveSpeed;
    public Vector2 wallJumpAngle;

    [Header("Audio Clip")]
    public AudioClip playerRun;
    public AudioClip playerJump;
    public AudioClip playerWallSlide;
    public AudioClip playerSlide;

    [Header("Particle System")]
    public ParticleSystem dust;

    private Rigidbody2D rigidbody2D;
    private Animator animator;
    private AudioSource movementAudio;
    private bool facing = true;
    private float moveValue;
    private bool jumpValue;
    private bool slideValue;
    private string moveAxisName;
    private string jumpAxisName;
    private KeyCode slideButtonName;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        movementAudio = GetComponent<AudioSource>();
        if (isServer) return;

        movementAudio.volume = PlayerPrefs.GetFloat("sfxVol", 0.4f);
    }

    private void Start()
    {
        moveAxisName = "Horizontal";
        jumpAxisName = "Jump";
        slideButtonName = KeyCode.S;
    }

    private void OnEnable()
    {
        moveValue = 0f;
    }


    private void OnDisable()
    {
        isSliding = false;

        if (!hasAuthority) return;
        CmdAudioStop();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        moveValue = Input.GetAxis(moveAxisName);
        jumpValue = Input.GetButtonDown(jumpAxisName);
        slideValue = Input.GetKeyDown(slideButtonName);
        CheckBool();

        Jump();
        Move();
        Sliding();
        WallSlide();
        WallJump();

        ControlAnimation();
    }

    public void CheckBool()
    {
        isTouchingWall = Physics2D.OverlapBox(wallCheckPoint.position, wallCheckSize, 0, wall);
    }

    private void ControlAnimation()
    {
        animator.SetBool("running", Mathf.Abs(moveValue) > 0);
        animator.SetBool("jumping", !isGrounded);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Ground")
        {
            isGrounded = true;
        }
    }

    private void Jump()
    {
        if (jumpValue && isGrounded)
        {
            rigidbody2D.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;
            CmdAudioStart(2, false, false);
        }
    }

    private void Move()
    {
        Vector3 movement = new Vector3(moveValue, 0f, 0f);
        transform.position += movement * Time.deltaTime * speed;
        if (Mathf.Abs(moveValue) != 0)
        {
            CmdAudioStart(1, true, false);
        }
        else
        {
            if (movementAudio.clip == playerRun)
            {
                CmdAudioStop();
            }
        }

        if (moveValue != 0)
        {
            CmdFlip(moveValue < 0);
            if (!dust.isPlaying)
            {
                CmdDust();
            }
        }
    }

    private void Sliding()
    {

        float xAngle = transform.eulerAngles.x;
        float yAngle = transform.eulerAngles.y;


        if (Input.GetKeyDown(KeyCode.S) && isGrounded)
        {
            isSliding = true;
            timeCount = 0;
            CmdAudioStart(4, false, false);
            CmdDust();
        }

        if (isSliding && timeCount < slideTime)
        {

            gameObject.transform.rotation = Quaternion.Euler(xAngle, yAngle, 90.0f);
            rigidbody2D.velocity = new Vector2((speed + slideForce) * -wallJumpDirection, rigidbody2D.velocity.y);
            timeCount += Time.deltaTime;

        }
        else
        {
            isSliding = false;
            gameObject.transform.rotation = Quaternion.Euler(xAngle, yAngle, 0.0f);

        }
    }

    private void WallSlide()
    {
        if (isTouchingWall && !isGrounded && rigidbody2D.velocity.y < 0)
        {
            isWallSlide = true;
            movementAudio.clip = playerWallSlide;
            CmdAudioStart(3, true, true);
        }
        else
        {
            isWallSlide = false;
        }

        if (isWallSlide)
        {
            rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, wallSlideSpeed);
        }
    }

    private void WallJump()
    {
        if ((isWallSlide || isTouchingWall) && Input.GetButtonDown("Jump"))
        {
            rigidbody2D.AddForce(new Vector2(wallJumpForce * wallJumpDirection * wallJumpAngle.x, wallJumpForce * wallJumpAngle.y), ForceMode2D.Impulse);
            CmdFlip(wallJumpDirection < 0);
        }
    }

    [Command]
    public void CmdFlip(bool flipStatus)
    {
        RpcFlip(flipStatus);
    }

    [ClientRpc]
    public void RpcFlip(bool flipStatus)
    {
        if (!isWallSlide)
        {
            facing = flipStatus;
            wallJumpDirection = flipStatus ? Mathf.Abs(wallJumpDirection) : Mathf.Abs(wallJumpDirection) * -1;
            GetComponent<SpriteRenderer>().flipX = facing;
        }
    }

    [Command]
    public void CmdAudioStart(int audioId, bool checkIsPlaying, bool loop)
    {
        RpcAudioStart(audioId, checkIsPlaying, loop);
    }

    [ClientRpc]
    public void RpcAudioStart(int audioId, bool checkIsPlaying, bool loop)
    {
        movementAudio.loop = loop;
        if (!checkIsPlaying || (checkIsPlaying && !movementAudio.isPlaying))
        {
            if (audioId == 1) movementAudio.clip = playerRun;
            else if (audioId == 2) movementAudio.clip = playerJump;
            else if (audioId == 3) movementAudio.clip = playerWallSlide;
            else if (audioId == 4) movementAudio.clip = playerSlide;

            movementAudio.Play();
        }
    }

    [Command]
    public void CmdAudioStop()
    {
        RpcAudioStop();
    }

    [ClientRpc]
    public void RpcAudioStop()
    {
        movementAudio.Stop();
    }

    [Command]
    public void CmdDust()
    {
        RpcDust();
    }

    [ClientRpc]
    public void RpcDust()
    {
        CreateDust();
    }

    private void CreateDust()
    {
        dust.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(wallCheckPoint.position, wallCheckSize);
    }

    private float IncreaseSpeedTo(float n, float target, float acc)
    {
        if (n == target)
        {
            return n;
        }
        else
        {
            float direction = Mathf.Sign(target - n);
            n += acc * Time.deltaTime * direction;
            if (direction == Mathf.Sign(target - n))
            {
                return n;
            }
            else
            {
                return target;
            }
        }
    }
}
