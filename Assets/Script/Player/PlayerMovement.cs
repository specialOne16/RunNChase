using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement and Position
    [Header("Player Movement")]
    public float speed;
    public float acceleration;

    private bool facing = true;
    private float moveDirection;
    private Vector3 initPosition;

    // Jumping
    [Header("Player Jumping")]
    public float jumpForce;
    public float jumpTime;
    [SerializeField] LayerMask ground;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] Vector2 groundCheckSize;

    private bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;

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

    // Slide
    [Header("Player Slide")]
    public float slideForce = 2.5f;
    public float slideTime= 0.5f;
    private bool isSliding = false;
    private float timeCount;

    // Other
    private Rigidbody2D rb2D;
    private AudioSource movementAudio;
    [Header("Game Manager")]
    public GameManager gameManager;
    
    [Header("Audio Clip")]
    public AudioClip playerRun;
    public AudioClip playerJump;
    public AudioClip playerWallSlide;
    public AudioClip playerSlide;

    [Header("Particle System")]
    public ParticleSystem dust;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        movementAudio = GetComponent<AudioSource>();
        wallJumpAngle.Normalize(); 
        initPosition = transform.position;
        RestartPosition();
    }

    private void Update()
    {
        moveDirection = Input.GetAxis("Horizontal");
        CheckBool();
    }

    private void FixedUpdate()
    {
        Movement();
        Jump();
        WallSlide();
        WallJump();
        Sliding();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.CompareTag("Chaser"))
        {
            if (collision.collider.CompareTag("Runner"))
            {
                gameManager.Tag();
            }
        }
    }

    public void RestartPosition()
    {
        transform.position = initPosition;
    }

    public void CheckBool()
    {
        isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, ground);
        isTouchingWall = Physics2D.OverlapBox(wallCheckPoint.position, wallCheckSize, 0, wall);
    }

    private void Movement()
    {
        if (isGrounded)
        {
            rb2D.velocity = new Vector2(moveDirection * speed, rb2D.velocity.y);
            if (Mathf.Abs(moveDirection) != 0)
            {
                movementAudio.clip = playerRun;
                if (!movementAudio.isPlaying)
                {
                    movementAudio.Play();
                }
            }
            else
            {
                movementAudio.Stop();
            }
        }
        else if (!isGrounded && (!isWallSlide || !isTouchingWall) && moveDirection != 0)
        {
            rb2D.AddForce(new Vector2(airMoveSpeed * moveDirection, 0));
            if (Mathf.Abs(rb2D.velocity.x) > speed)
            {
                rb2D.velocity = new Vector2(moveDirection * speed, rb2D.velocity.y);
            }
        }

        if (moveDirection < 0 && facing)
        {
            Flip();
            CreateDust();
        }
        else if (moveDirection > 0 && !facing)
        {
            Flip();
            CreateDust();
        }
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce);
            movementAudio.clip = playerJump;
            movementAudio.Play();
            CreateDust();
        }

        if (Input.GetButton("Jump"))
        {
            if (!movementAudio.isPlaying)
            {
                movementAudio.clip = playerJump;
                movementAudio.Play();
            }
            if (jumpTimeCounter > 0 && isJumping)
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
                CreateDust();
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
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
            movementAudio.clip = playerSlide;
            movementAudio.Play();
            CreateDust();
        }

        if (isSliding && timeCount < slideTime)
        {

            gameObject.transform.rotation = Quaternion.Euler(xAngle, yAngle, 90.0f);
            rb2D.velocity = new Vector2((speed + slideForce)*-wallJumpDirection, rb2D.velocity.y);
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
        if (isTouchingWall && !isGrounded && rb2D.velocity.y < 0)
        {
            isWallSlide = true;
            movementAudio.clip = playerWallSlide;
            movementAudio.loop = true;
            if (!movementAudio.isPlaying)
            {
                movementAudio.Play();
            }
        }
        else
        {
            movementAudio.loop = false;
            isWallSlide = false;
        }

        if (isWallSlide)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, wallSlideSpeed);
        }
    }

    private void WallJump()
    {
        if ((isWallSlide || isTouchingWall) && Input.GetButtonDown("Jump"))
        {
            rb2D.AddForce(new Vector2(wallJumpForce * wallJumpDirection * wallJumpAngle.x, wallJumpForce * wallJumpAngle.y), ForceMode2D.Impulse);
        }
    }


    void Flip()
    {
        if (!isWallSlide)
        {
            wallJumpDirection *= -1;
            facing = !facing;
            transform.Rotate(0, 180, 0);
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(groundCheckPoint.position, groundCheckSize);

        Gizmos.color = Color.red;
        Gizmos.DrawCube(wallCheckPoint.position, wallCheckSize);
    }

    private void CreateDust()
    {
        dust.Play();
    }
}
