using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.MultiplayerModels;
using PlayFab.ProfilesModels;
using PlayFab.ClientModels;
using System.Collections.Generic;

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

    private PlayfabLogin loginManager;
    private string ticketId;
    private Coroutine pollTicketCoroutine;

    [HideInInspector] public string enemyName = "";
    [HideInInspector] public string serverIp = "";
    [HideInInspector] public int serverPort = 0;

    public void OnPageActive()
    {
        loginManager = gameObject.GetComponent<PlayfabLogin>();
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

        PlayFabMultiplayerAPI.ListQosServersForTitle(new ListQosServersForTitleRequest(), qosRes =>
        {
            var qosServer = qosRes.QosServers[0].ServerUrl;
            var qosRegion = qosRes.QosServers[0].Region;
            Debug.Log($"Pinging QoS Server {qosServer} at {qosRegion}");
            Debug.Log(qosRes.ToJson());

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var udpPort = 5600;
            var done = false;
            while (!done || udpPort > 5610)
            {
                try
                {
                    UdpClient client = new UdpClient(udpPort);
                    client.Connect(qosServer, 3075);
                    byte[] sendBytes = BitConverter.GetBytes(0xFFFF);
                    client.Send(sendBytes, sendBytes.Length);

                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 3075);
                    byte[] receiveBytes = client.Receive(ref remoteEndPoint);
                    client.Close();
                    done = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[QoS Ping Error]: {e.Message}");
                    udpPort++;
                    Debug.Log($"Retrying with port {udpPort}");
                }
            }
            var pingTime = sw.ElapsedMilliseconds;
            Debug.Log($"Ping success with {pingTime}ms");
            if (udpPort > 5610)
            {
                StartMatchmaking();
                return;
            }

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
                        DataObject = new LatenciesData
                        {
                            Latencies = new List<Latency>
                            {
                                { new Latency { region = qosRegion, latency = pingTime } }
                            }
                        }
                    }
                },
                GiveUpAfterSeconds = PlayfabUtils.MATCHMAKING_TIMEOUT,
                QueueName = PlayfabUtils.MATCHMAKING_NAME
            };

            PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnTicketCreated, OnError);
        }, OnError);
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
            case "Canceled":
                PlayfabUtils.OnError(feedbackText, "Matchmaking timeout! Please try again...");
                StopCoroutine(pollTicketCoroutine);
                leaveBtn.SetActive(false);
                backBtn.GetComponent<Button>().interactable = true;
                break;
        }
    }

    private void PrepareMatch(string matchId)
    {
        var request = new GetMatchRequest
        {
            MatchId = matchId,
            QueueName = PlayfabUtils.MATCHMAKING_NAME,

        };

        PlayFabMultiplayerAPI.GetMatch(request, OnMatchGet, OnError);
    }

    private void OnMatchGet(GetMatchResult res)
    {
        serverIp = res.ServerDetails.IPV4Address;
        serverPort = res.ServerDetails.Ports[0].Num;
        Debug.Log($"Server Details - IP:{serverIp} | Port:{serverPort}");

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
        SceneManager.LoadSceneAsync(1);
    }

    private void OnError(PlayFabError error)
    {
        PlayfabUtils.OnError(feedbackText, error.ErrorMessage);
    }
}

[Serializable]
public class LatenciesData
{
    public List<Latency> Latencies;
}

[Serializable]
public class Latency
{
    public string region;
    public long latency;
}