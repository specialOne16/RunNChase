using UnityEngine;
using Mirror;

public class NetPlayerMovement : NetworkBehaviour
{
    [Header("Game Configuration")]
    [HideInInspector] public int playerNumber = 1;

    [Header("Physics Configuration")]
    public float speed = 10;
    public float acceleration = 15;
    public float jumpForce = 5;
    public bool isGrounded = false;

    private Rigidbody2D rigidbody2D;
    private Animator animator;
    private float moveValue;
    private bool jumpValue;
    private string moveAxisName;
    private string jumpAxisName;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        moveAxisName = "Horizontal";
        jumpAxisName = "Jump";
    }

    private void OnEnable()
    {
        moveValue = 0f;
    }


    private void OnDisable()
    {
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        moveValue = Input.GetAxis(moveAxisName);
        jumpValue = Input.GetButtonDown(jumpAxisName);

        Jump();
        Move();

        ControlAnimation();
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
        }
    }

    private void Move()
    {
        Vector3 movement = new Vector3(moveValue, 0f, 0f);
        transform.position += movement * Time.deltaTime * speed;
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
