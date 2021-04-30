using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : MonoBehaviour
{
    GameObject player;

    void Start()
    {
        player = gameObject.transform.parent.gameObject;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Ground")
        {
            player.GetComponent<PlayerMovement>().isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Ground")
        {
            player.GetComponent<PlayerMovement>().isGrounded = false;
        }
    }
}
