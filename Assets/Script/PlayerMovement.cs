using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int speed;
    public GameManager gameManager;

    private Vector3 initPosition;
    private Vector3 velocity;

    void Start()
    {
        initPosition = transform.position;
        RestartPosition();
    }

    private void Update()
    {
        transform.position = transform.position + velocity * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.CompareTag("Chaser"))
        {
            if (collision.collider.CompareTag("Runner"))
            {
                gameManager.Tag(gameObject.name);
            }
        }
    }

    public void RestartPosition()
    {
        transform.position = initPosition;
        velocity = Vector3.right * speed;
    }
}
