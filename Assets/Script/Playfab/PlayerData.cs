using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountInfo
{
    public string name;
    public string email;
    public string playfabId;
    public string entityId;
    public string sessionTicket;
};

[Serializable]
public class PlayerData
{
    [Serializable]
    public class PowerUps
    {
        public int sprintTicket;
        public int marathonTicket;
        public int foodCoupon;
        public int milkCoupon;
        public int exchangeProgram;
    };

    [Serializable]
    public class Stats
    {
        public int maxSpeed;
        public int minStamina;
        public int width;
        public int height;
        public int rankPoints;
    };

    public PowerUps powerUps;
    public Stats stats;
    [NonSerialized] public AccountInfo accountInfo;

    public PlayerData()
    {
        // Default Values
        stats = new Stats
        {
            maxSpeed = 1,
            minStamina = 1,
            width = 1,
            height = 1,
            rankPoints = 1
        };
        powerUps = new PowerUps
        {
            sprintTicket = 0,
            marathonTicket = 0,
            foodCoupon = 0,
            milkCoupon = 0,
            exchangeProgram = 0
        };
        accountInfo = new AccountInfo
        {
            name = "",
            email = "",
            playfabId = "",
            sessionTicket = "",
            entityId = ""
        };
    }

    public void setAccountInfo(string name, string email, string playfabId, string sessionTicket, string entityId)
    {
        accountInfo = new AccountInfo
        {
            name = name,
            email = email,
            playfabId = playfabId,
            sessionTicket = sessionTicket,
            entityId = entityId
        };
    }

    public void LoadJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

    static public PlayerData FromJson(string json)
    {
        return JsonUtility.FromJson<PlayerData>(json);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
