using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[Serializable]
public class PlayerData
{
    public PlayerData(string name, int size)
    {
        this.name = name;
        this.size = size;
    }

    public string name;
    public int size;
}

public class PlayfabLoginRegister : MonoBehaviour
{
    [Header("Text Feedback Fields")]
    public Text feedbackText;
    public Text usernameText;

    [Header("Input Fields")]
    public InputField name;
    public InputField email;
    public InputField password;

    private Boolean hasLogin = false;
    private string displayName = "";

    private void ClearTextFields()
    {
        name.text = "";
        email.text = "";
        password.text = "";
    }

    private void Start()
    {
        feedbackText.text = "";
        usernameText.text = "You haven't login yet";
    }

    public void Register()
    {
        if (email.text.Equals("") || name.text.Equals("") || password.text.Equals(""))
        {
            OnError("Register needs email, name, and password!");
            return;
        }
        if (password.text.Length < 6)
        {
            OnError("Password needs to be at least 6 characters!");
            return;
        }
        var request = new RegisterPlayFabUserRequest
        {
            Email = email.text,
            Password = password.text,
            DisplayName = name.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        feedbackText.color = new Color(0.75f, 1, 0.75f, 1);
        feedbackText.text = "Register success!";
        ClearTextFields();
    }

    public void Login()
    {
        if (email.text.Equals("") || password.text.Equals(""))
        {
            OnError("Login needs email and password!");
            return;
        }
        if (password.text.Length < 6)
        {
            OnError("Password needs to be at least 6 characters!");
            return;
        }
        var request = new LoginWithEmailAddressRequest
        {
            Email = email.text,
            Password = password.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        feedbackText.color = new Color(0.75f, 1, 0.75f, 1);
        feedbackText.text = "Login success!";
        usernameText.text = result.InfoResultPayload.PlayerProfile.DisplayName;
        hasLogin = true;
        displayName = result.InfoResultPayload.PlayerProfile.DisplayName;
        ClearTextFields();
    }

    public void ResetPassword()
    {
        if (email.text.Equals(""))
        {
            OnError("Reset Password needs email!");
            return;
        }
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = email.text,
            TitleId = "BD903"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordRecoverySent, OnError);
    }

    private void OnPasswordRecoverySent(SendAccountRecoveryEmailResult result)
    {
        feedbackText.color = new Color(0.75f, 1, 0.75f, 1);
        feedbackText.text = "Email for password reset sent!";
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
}
