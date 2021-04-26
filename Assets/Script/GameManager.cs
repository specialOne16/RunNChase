using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] int scoreP1 = 0;
    [SerializeField] int scoreP2 = 0;

    public GameObject player1;
    public GameObject player2;

    private PlayerMovement player1Movement;
    private PlayerMovement player2Movement;
    private int match = 0;

    private void Start()
    {
        player1Movement = player1.GetComponent<PlayerMovement>();
        player2Movement = player2.GetComponent<PlayerMovement>();
    }

    public void Tag(string chaser)
    {
        Debug.Log($"{chaser} catches!");

        player1Movement.RestartPosition();
        player2Movement.RestartPosition();

        player1Movement.enabled = false;
        player2Movement.enabled = false;

        match++;

        StartCoroutine("ResumeMatch");

    }

    IEnumerator ResumeMatch()
    {
        yield return new WaitForSeconds(1f);

        player1Movement.enabled = true;
        player2Movement.enabled = true;

        if(match%2 == 0)
        {
            player1.tag = "Chaser";
            player2.tag = "Runner";
        }
        else
        {
            player1.tag = "Runner";
            player2.tag = "Chaser";
        }
    }
}
