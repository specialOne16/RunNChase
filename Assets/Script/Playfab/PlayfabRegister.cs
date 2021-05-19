using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class PlayfabRegister : MonoBehaviour
{
    [Header("Text Feedback Fields")]
    public Text feedbackText;
    public Text usernameText;

    [Header("Input Fields")]
    public InputField nameInput;
    public InputField emailInput;
    public InputField passwordInput;

    private PlayfabLogin loginManager;

    private void ClearTextFields()
    {
        nameInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
    }

    public void OnPageActive()
    {
        loginManager = gameObject.GetComponent<PlayfabLogin>();

        if (!loginManager.hasLogin)
        {
            feedbackText.text = "";
            usernameText.text = "You haven't login yet";
        } else
        {
            usernameText.text = loginManager.playerData.accountInfo.name;
        }
    }

    public void Register()
    {
        if (emailInput.text.Equals("") || nameInput.text.Equals("") || passwordInput.text.Equals(""))
        {
            PlayfabUtils.OnError(feedbackText, "Register needs email, name, and password!");
            return;
        }
        if (passwordInput.text.Length < 6)
        {
            PlayfabUtils.OnError(feedbackText, "Password needs to be at least 6 characters!");
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
        PlayfabUtils.OnSuccess(feedbackText, "Register success!");
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


    private void OnError(PlayFabError error)
    {
        PlayfabUtils.OnError(feedbackText, error.ErrorMessage);
    }
}
