using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10;
    public float acceleration = 15;
    public float jumpForce = 5;
    public GameManager gameManager;
    public bool isGrounded = false;

    private Vector3 initPosition;
    private Rigidbody2D rigidbody2D;

    void Start()
    {
        initPosition = transform.position;
        rigidbody2D = GetComponent<Rigidbody2D>();
        RestartPosition();
    }

    private void Update()
    {
        Jump();
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
        transform.position += movement * Time.deltaTime * speed;
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

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigidbody2D.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
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
