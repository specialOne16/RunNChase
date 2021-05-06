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

    public float matchDuration = 10;
    private float timeRemaining;
    private bool timerIsRunning;

    private void Start()
    {
        player1Movement = player1.GetComponent<PlayerMovement>();
        player2Movement = player2.GetComponent<PlayerMovement>();
        timerIsRunning = true;
        timeRemaining = matchDuration;
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                GetAway();
            }
        }
    }

    public void Tag()
    {
        if (player1.tag == "Chaser")
        {
            Debug.Log($"{player1.name} catches!");
        }
        else
        {
            Debug.Log($"{player2.name} catches!");
        }

        player1Movement.RestartPosition();
        player2Movement.RestartPosition();

        player1Movement.enabled = false;
        player2Movement.enabled = false;

        match++;

        StartCoroutine("ResumeMatch");

    }

    public void GetAway()
    {
        if(player1.tag == "Chaser")
        {
            Debug.Log($"{player2.name} get away!");
            scoreP2++;
        }
        else
        {
            Debug.Log($"{player1.name} get away!");
            scoreP1++;
        }

        player1Movement.RestartPosition();
        player2Movement.RestartPosition();

        player1Movement.enabled = false;
        player2Movement.enabled = false;

        match++;

        StartCoroutine("ResumeMatch");

    }

    IEnumerator ResumeMatch()
    {
        timerIsRunning = false;
        timeRemaining = 0;

        yield return new WaitForSeconds(3f);

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

        timerIsRunning = true;
        timeRemaining = matchDuration;
    }

    private void OnGUI()
    {
        GUI.TextArea(new Rect(Screen.width / 2 - 200, Screen.height/4, 400, 110), $"{Mathf.RoundToInt(timeRemaining)} \n{scoreP1} - {scoreP2}", new GUIStyle(GUI.skin.textArea));
    }
}
