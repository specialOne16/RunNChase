using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class PlayfabLoginRegister : MonoBehaviour
{
    [Header("Text Feedback Fields")]
    public Text feedbackText;
    public Text usernameText;

    [Header("Input Fields")]
    public InputField nameInput;
    public InputField emailInput;
    public InputField passwordInput;

    private Boolean hasLogin = false;
    public PlayerData playerData = new PlayerData();

    private void ClearTextFields()
    {
        nameInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
    }

    public void OnPageActive()
    {
        if (!hasLogin)
        {
            feedbackText.text = "";
            usernameText.text = "You haven't login yet";
        } else
        {
            usernameText.text = playerData.accountInfo.name;
        }
    }

    public void Register()
    {
        if (emailInput.text.Equals("") || nameInput.text.Equals("") || passwordInput.text.Equals(""))
        {
            OnError("Register needs email, name, and password!");
            return;
        }
        if (passwordInput.text.Length < 6)
        {
            OnError("Password needs to be at least 6 characters!");
            return;
        }
        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,
            DisplayName = nameInput.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        OnSuccess("Register success!");
        SendPlayerData();
        ClearTextFields();
    }

    private void SendPlayerData()
    {
        var newPlayerData = new PlayerData();
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "PlayerData", newPlayerData.ToJson() }
            }
        };
        PlayFabClientAPI.UpdateUserData(request, OnDataSent, OnError);
    }

    private void OnDataSent(UpdateUserDataResult res)
    {
        Debug.Log("New Player data sent!");
    }

    public void Login()
    {
        if (emailInput.text.Equals("") || passwordInput.text.Equals(""))
        {
            OnError("Login needs email and password!");
            return;
        }
        if (passwordInput.text.Length < 6)
        {
            OnError("Password needs to be at least 6 characters!");
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
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        OnSuccess("Login success!");
        usernameText.text = result.InfoResultPayload.PlayerProfile.DisplayName;
        hasLogin = true;
        var displayName = result.InfoResultPayload.PlayerProfile.DisplayName;
        var email = result.InfoResultPayload.AccountInfo.PrivateInfo.Email;
        var playfabID = result.InfoResultPayload.AccountInfo.PlayFabId;
        if (result.InfoResultPayload.UserData != null && result.InfoResultPayload.UserData.ContainsKey("PlayerData"))
        {
            playerData = PlayerData.FromJson(result.InfoResultPayload.UserData["PlayerData"].Value);
        } else
        {
            playerData = new PlayerData();
        }
        
        playerData.setAccountInfo(displayName, email, playfabID);
        ClearTextFields();
    }

    public void ResetPassword()
    {
        if (emailInput.text.Equals(""))
        {
            OnError("Reset Password needs email!");
            return;
        }
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailInput.text,
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
