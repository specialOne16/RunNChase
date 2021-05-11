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

    // Other
    private bool facing = true;
    private Rigidbody2D rb2D;
    public GameManager gameManager;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        wallJumpAngle.Normalize();
        initPosition = transform.position;
        RestartPosition();
    }

    private void Update()
    {
        moveDirection = Input.GetAxis("Horizontal");
        Movement();
        Jump();
        WallSlide();
        WallJump();
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
