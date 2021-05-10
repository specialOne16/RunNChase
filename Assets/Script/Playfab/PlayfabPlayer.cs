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
            PlayfabUtils.OnError(feedbackText, "You must login first to see the player data!");
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

            PlayfabUtils.OnSuccess(feedbackText, "Player Data Updated!");
        }
    }

    public void UpdateDisplayName()
    {
        if (!loginManager.isLoggedIn())
        {
            PlayfabUtils.OnError(feedbackText, "Please login first!");
            return;
        }
        if (nameInput.text == "")
        {
            PlayfabUtils.OnError(feedbackText, "New Display Name must not be empty!");
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

        PlayfabUtils.OnSuccess(feedbackText, "Display Name Updated!");
    }

    public void ResetPassword()
    {
        if (loginManager.getEmail() == "")
        {
            PlayfabUtils.OnError(feedbackText, "Reset Password needs login!");
            return;
        }
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = loginManager.getEmail(),
            TitleId = PlayfabUtils.TITLE_ID
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordRecoverySent, OnError);
    }

    private void OnPasswordRecoverySent(SendAccountRecoveryEmailResult result)
    {
        PlayfabUtils.OnSuccess(feedbackText, "Email for password reset sent!");
    }

    private void OnError(PlayFabError error)
    {
        PlayfabUtils.OnError(feedbackText, error.ErrorMessage);
    }
}
