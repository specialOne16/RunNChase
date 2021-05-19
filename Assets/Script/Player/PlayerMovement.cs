using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Movement and Position
    public float speed;
    public float acceleration;

    private float moveDirection;
    private Vector3 initPosition;
    private Quaternion initRotation;

    // Jumping
    public float jumpForce;
    public float jumpTime;
    public bool isGrounded;

    private bool isJumping;
    private float jumpTimeCounter;

    // Wall Slide
    public float wallSlideSpeed;
    public bool isTouchingWall;

    private bool isWallSlide;

    // Wall Jump
    public float wallJumpForce;
    public float wallJumpDirection = -1f;
    public float airMoveSpeed;
    public Vector2 wallJumpAngle;

    // slide
    public float slideForce = 2.5f;
    public float slideTime= 0.5f;
    private bool isSliding = false;
    private float timeCount;


    // Player Data
    public float K = 1;
    private PlayerData playerData = new PlayerData();
    private float playerSpeed;
    private float playerStamina;
    private float playerSize;
    private float playerXSize;
    private float playerYSize;
    private float minStamina;
    private float maxStamina;
    

    // Other
    private bool facing = true;
    private bool matchStart = false;
    private Rigidbody2D rb2D;
    public GameManager gameManager;

    private void Awake()
    {
        playerData.stats.height = 2;
        playerData.stats.width = 3;
        playerData.stats.maxSpeed = 5;
        playerData.stats.minStamina = 6;

        playerXSize = playerData.stats.width;
        playerYSize = playerData.stats.height;
        playerSize = playerXSize * playerYSize;

        minStamina = playerData.stats.minStamina + K * playerSize * Random.Range(1, gameManager.getScore(gameObject)+1);

        maxStamina = minStamina;

    }

    void Start()
    {
        playerStamina = minStamina + playerSize * 5 / 8;
        rb2D = GetComponent<Rigidbody2D>();
        wallJumpAngle.Normalize(); 
        initPosition = transform.position;
        initRotation = transform.rotation;
        RestartPosition();
    }

    private void Update()
    {
        moveDirection = Input.GetAxis("Horizontal");
        Movement();
        Jump();
        WallSlide();
        WallJump();
        sliding();
        
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
        float x = Random.Range(1, gameManager.getScore(gameObject)+1);
        playerSpeed = Map(playerData.stats.maxSpeed * Random.Range(0.5f, 1.5f), 0, 9, 9, 16); 
        if (playerSpeed < 0) playerSpeed = 0;
        speed = playerSpeed;

        float mult;
        if (gameObject.tag.Equals("Chaser"))
        {
            mult = 1.2f;
        }
        else
        {
            mult = 0.9f;
        }

        if (gameManager.getRoundDuration() < 5.0f)
        {
            playerStamina -= Mathf.Sqrt(playerSize) * 5 * mult / 8;
        }
        else
        {
            playerStamina -= Mathf.Sqrt(playerSize) * Random.Range(gameManager.getRoundDuration() - 5.0f, gameManager.getRoundDuration()) * mult;
        }
        if (playerStamina < 0)
        {
            playerStamina = 0;
        }
        minStamina = playerStamina;
        transform.position = initPosition;
        transform.rotation = initRotation;
    }

    private float Map(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    private void Movement()
    {
        if (isGrounded)
        {
            rb2D.velocity = new Vector2(moveDirection * speed, rb2D.velocity.y);
        }
        else if (!isGrounded && !isWallSlide && moveDirection != 0)
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
        }
        else if (moveDirection > 0 && !facing)
        {
            Flip();
        }
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce);
        }

        if (Input.GetButton("Jump"))
        {
            if (jumpTimeCounter > 0 && isJumping)
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
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
    
    private void sliding()
    {

        float xAngle = transform.eulerAngles.x;
        float yAngle = transform.eulerAngles.y;


        if (Input.GetKeyDown(KeyCode.S) && isGrounded)
        {
            isSliding = true;
            timeCount = 0;
            
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
        }
        else
        {
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
}
