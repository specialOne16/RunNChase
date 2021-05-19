using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class PlayfabLogin : MonoBehaviour
{
    [Header("Text Feedback Fields")]
    public Text feedbackText;
    public Text usernameText;

    [Header("Input Fields")]
    public InputField emailInput;
    public InputField passwordInput;

    public Boolean hasLogin = false;
    public PlayerData playerData = new PlayerData();

    private void ClearTextFields()
    {
        emailInput.text = "";
        passwordInput.text = "";
    }

    public void OnPageActive()
    {
        if (!hasLogin)
        {
            feedbackText.text = "";
            usernameText.text = "You haven't login yet";
        }
        else
        {
            usernameText.text = playerData.accountInfo.name;
        }
    }

    public void Login()
    {
        Debug.Log("Logging in");
        Debug.Log(emailInput.text);
        Debug.Log(passwordInput.text);
        if (emailInput.text.Equals("") || passwordInput.text.Equals(""))
        {
            PlayfabUtils.OnError(feedbackText, "Login needs email and password!");
            return;
        }
        if (passwordInput.text.Length < 6)
        {
            PlayfabUtils.OnError(feedbackText, "Password needs to be at least 6 characters!");
            return;
        }
        var request = new LoginWithEmailAddressRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetUserData = true,
                GetUserAccountInfo = true
            }
        };
        Debug.Log("Sending the data...");
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        PlayfabUtils.OnSuccess(feedbackText, "Login success!");
        usernameText.text = result.InfoResultPayload.PlayerProfile.DisplayName;
        hasLogin = true;
        var displayName = result.InfoResultPayload.PlayerProfile.DisplayName;
        var email = result.InfoResultPayload.AccountInfo.PrivateInfo.Email;
        var playfabId = result.InfoResultPayload.AccountInfo.PlayFabId;
        var sessionTicket = result.SessionTicket;
        var entityId = result.EntityToken.Entity.Id;
        if (result.InfoResultPayload.UserData != null && result.InfoResultPayload.UserData.ContainsKey("PlayerData"))
        {
            playerData = PlayerData.FromJson(result.InfoResultPayload.UserData["PlayerData"].Value);
        }
        else
        {
            playerData = new PlayerData();
        }

        playerData.setAccountInfo(displayName, email, playfabId, sessionTicket, entityId);
        ClearTextFields();
    }

    public void ResetPassword()
    {
        if (emailInput.text.Equals(""))
        {
            PlayfabUtils.OnError(feedbackText, "Reset Password needs email!");
            return;
        }
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailInput.text,
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

    public Boolean isLoggedIn()
    {
        return hasLogin;
    }

    public String getDisplayName()
    {
        return playerData.accountInfo.name;
    }

    public String getEmail()
    {
        return playerData.accountInfo.email;
    }
}
