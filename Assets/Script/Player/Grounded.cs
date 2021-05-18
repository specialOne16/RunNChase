using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : MonoBehaviour
{
    GameObject player;
    public LayerMask ground;
    public Transform groundCheckPoint;
    public Vector2 groundCheckSize;

    void Start()
    {
        player = gameObject.transform.parent.gameObject;
    }

    private void Update()
    {
        //player.GetComponent<PlayerMovement>().isGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0, ground);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(groundCheckPoint.position, groundCheckSize);
    }
}
