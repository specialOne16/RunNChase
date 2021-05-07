using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabPlayer : MonoBehaviour
{

    [Header("Text Feedback Fields")]
    public Text feedbackText;
    public Text nameText;

    [Header("Power Ups Texts")]
    public Text sprintTicketText;
    public Text marathonTicketText;
    public Text foodCouponText;
    public Text milkCouponText;
    public Text exchangeProgramText;

    [Header("Stats Texts")]
    public Text speedText;
    public Text staminaText;
    public Text widthText;
    public Text heightText;
    public Text rankPointsText;

    [Header("Input Fields")]
    public InputField nameInput;

    private PlayfabLoginRegister loginManager;

    public void OnPageActive()
    {
        loginManager = gameObject.GetComponent<PlayfabLoginRegister>();
        feedbackText.text = "";

        nameText.text = loginManager.getDisplayName();
        if (loginManager.isLoggedIn())
        {
            GetPlayerData();
        }
        else
        {
            OnError("You must login first to see the player data!");
        }
    }

    public void GetPlayerData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnDataGet, OnError);
    }

    private void OnDataGet(GetUserDataResult res)
    {
        if (res.Data != null && res.Data.ContainsKey("PlayerData"))
        {
            loginManager.playerData.LoadJson(res.Data["PlayerData"].Value);

            sprintTicketText.text = loginManager.playerData.powerUps.sprintTicket.ToString();
            marathonTicketText.text = loginManager.playerData.powerUps.marathonTicket.ToString();
            foodCouponText.text = loginManager.playerData.powerUps.foodCoupon.ToString();
            milkCouponText.text = loginManager.playerData.powerUps.milkCoupon.ToString();
            exchangeProgramText.text = loginManager.playerData.powerUps.exchangeProgram.ToString();

            speedText.text = loginManager.playerData.stats.maxSpeed.ToString();
            staminaText.text = loginManager.playerData.stats.minStamina.ToString();
            widthText.text = loginManager.playerData.stats.width.ToString();
            heightText.text = loginManager.playerData.stats.height.ToString();
            rankPointsText.text = loginManager.playerData.stats.rankPoints.ToString();

            OnSuccess("Player Data Updated!");
        }
    }

    public void UpdateDisplayName()
    {
        if (!loginManager.isLoggedIn())
        {
            OnError("Please login first!");
            return;
        }
        if (nameInput.text == "")
        {
            OnError("New Display Name must not be empty!");
            return;
        }
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nameInput.text
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnNameUpdated, OnError);
    }

    private void OnNameUpdated(UpdateUserTitleDisplayNameResult res)
    {
        loginManager.playerData.accountInfo.name = res.DisplayName;
        nameText.text = res.DisplayName;

        nameInput.text = "";

        OnSuccess("Display Name Updated!");
    }

    public void ResetPassword()
    {
        if (loginManager.getEmail() == "")
        {
            OnError("Reset Password needs login!");
            return;
        }
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = loginManager.getEmail(),
            TitleId = "BD903"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordRecoverySent, OnError);
    }

    private void OnPasswordRecoverySent(SendAccountRecoveryEmailResult result)
    {
        OnSuccess("Email for password reset sent!");
    }

    private void OnError(PlayFabError error)
    {
        feedbackText.color = new Color(1, 0.75f, 0.75f, 1);
        feedbackText.text = error.ErrorMessage;
    }

    private void OnError(string error)
    {
        feedbackText.color = new Color(1, 0.75f, 0.75f, 1);
        feedbackText.text = error;
    }

    private void OnSuccess(string success)
    {
        feedbackText.color = new Color(0.75f, 1, 0.75f, 1);
        feedbackText.text = success;
    }
}
