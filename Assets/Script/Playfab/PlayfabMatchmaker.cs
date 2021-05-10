using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.MultiplayerModels;
using PlayFab.ProfilesModels;
using PlayFab.ClientModels;
using System.Collections;

public class PlayfabMatchmaker : MonoBehaviour
{
    [Header("Text Fields")]
    public Text feedbackText;
    public Text usernameText;
    public Text enemyText;

    [Header("Buttons")]
    public GameObject leaveBtn;
    public GameObject playBtn;
    public GameObject backBtn;

    private PlayfabLoginRegister loginManager;
    private string ticketId;
    private Coroutine pollTicketCoroutine;

    [HideInInspector]
    public string enemyName = "";

    public void OnPageActive()
    {
        loginManager = gameObject.GetComponent<PlayfabLoginRegister>();
        feedbackText.text = "";
        leaveBtn.SetActive(false);
        playBtn.SetActive(false);

        usernameText.text = loginManager.getDisplayName();
        enemyText.text = "Loading...";
        if (loginManager.isLoggedIn())
        {
            backBtn.GetComponent<Button>().interactable = false;
            StartMatchmaking();
        }
        else
        {
            backBtn.GetComponent<Button>().interactable = true;
            PlayfabUtils.OnError(feedbackText, "You must login first to start matchmaking!");
        }
    }

    public void StartMatchmaking()
    {
        PlayfabUtils.OnSuccess(feedbackText, "Matchmaking in progress...");

        var request = new CreateMatchmakingTicketRequest
        {
            Creator = new MatchmakingPlayer
            {
                Entity = new PlayFab.MultiplayerModels.EntityKey
                {
                    Id = loginManager.playerData.accountInfo.entityId,
                    Type = PlayfabUtils.ENTITY_TYPE
                },
                Attributes = new MatchmakingPlayerAttributes
                {
                    DataObject = new { }
                }
            },
            GiveUpAfterSeconds = PlayfabUtils.MATCHMAKING_TIMEOUT,
            QueueName = PlayfabUtils.MATCHMAKING_NAME
        };

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnTicketCreated, OnError);
    }

    private void OnTicketCreated(CreateMatchmakingTicketResult res)
    {
        ticketId = res.TicketId;
        leaveBtn.SetActive(true);

        PlayfabUtils.OnSuccess(feedbackText, "Match Ticket Created!");
        pollTicketCoroutine = StartCoroutine(PollTicket());
    }

    private IEnumerator PollTicket()
    {
        while (true)
        {
            var request = new GetMatchmakingTicketRequest
            {
                TicketId = ticketId,
                QueueName = PlayfabUtils.MATCHMAKING_NAME
            };
            PlayFabMultiplayerAPI.GetMatchmakingTicket(request, OnTicketGet, OnError);

            yield return new WaitForSeconds(PlayfabUtils.MATCHMAKING_POLL_INTERVAL);
        }
    }

    private void OnTicketGet(GetMatchmakingTicketResult res)
    {
        PlayfabUtils.OnSuccess(feedbackText, $"Matchmaking Status: {res.Status}");

        switch(res.Status)
        {
            case "Matched":
                StopCoroutine(pollTicketCoroutine);
                leaveBtn.SetActive(false);
                PrepareMatch(res.MatchId);
                break;
            case "Cancelled":
                Debug.Log("Getting Cancelled!");
                StopCoroutine(pollTicketCoroutine);
                break;
        }
    }

    private void PrepareMatch(string matchId)
    {
        var request = new GetMatchRequest
        {
            MatchId = matchId,
            QueueName = PlayfabUtils.MATCHMAKING_NAME
        };

        PlayFabMultiplayerAPI.GetMatch(request, OnMatchGet, OnError);
    }

    private void OnMatchGet(GetMatchResult res)
    {
        int enemyIdx = res.Members.FindIndex(x => x.Entity.Id != loginManager.playerData.accountInfo.entityId);
        var enemyEntity = res.Members[enemyIdx].Entity;

        // Getting enemy name from entityid -> playfabid -> profilename
        var entityRequest = new GetEntityProfileRequest
        {
            Entity = new PlayFab.ProfilesModels.EntityKey
            {
                Id = enemyEntity.Id,
                Type = enemyEntity.Type
            }
        };
        PlayFabProfilesAPI.GetProfile(entityRequest, entityRes =>
        {
            var enemyPlayfabId = entityRes.Profile.Lineage.MasterPlayerAccountId;
            var profileRequest = new GetPlayerProfileRequest
            {
                PlayFabId = enemyPlayfabId
            };
            PlayFabClientAPI.GetPlayerProfile(profileRequest, profileRes =>
            {
                enemyName = profileRes.PlayerProfile.DisplayName;
                StartMatch();
            }, OnError);
        }, OnError);
    }

    public void CancelMatchmaking()
    {
        leaveBtn.SetActive(false);
        var request = new CancelMatchmakingTicketRequest
        {
            TicketId = ticketId,
            QueueName = PlayfabUtils.MATCHMAKING_NAME
        };
        PlayFabMultiplayerAPI.CancelMatchmakingTicket(request, OnTicketCancelled, OnError);
    }

    private void OnTicketCancelled(CancelMatchmakingTicketResult res)
    {
        StopCoroutine(pollTicketCoroutine);
        backBtn.GetComponent<Button>().interactable = true;
        PlayfabUtils.OnSuccess(feedbackText, "Matchmaking cancelled!");
    }

    private void StartMatch()
    {
        enemyText.text = enemyName;
        playBtn.SetActive(true);
        backBtn.GetComponent<Button>().interactable = true;
        PlayfabUtils.OnSuccess(feedbackText, "Match started!");
    }

    public void OnPlay()
    {
        PlayfabUtils.OnSuccess(feedbackText, "Blom ada gameplaynya bapak");
    }

    private void OnError(PlayFabError error)
    {
        PlayfabUtils.OnError(feedbackText, error.ErrorMessage);
    }
}
