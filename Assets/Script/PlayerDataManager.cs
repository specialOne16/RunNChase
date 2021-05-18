using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataManager : MonoBehaviour
{
    public Text speed;
    public Text stamina;
    public Text width;
    public Text height;

    public Text sprint;
    public Text marathon;
    public Text food;
    public Text milk;

    public Text exchange;

    private PlayfabLogin loginManager;
    private PlayfabPlayer playerManager;

    private void Awake()
    {
        loginManager = GameObject.Find("PlayfabManager").GetComponent<PlayfabLogin>();
        playerManager = GameObject.Find("PlayfabManager").GetComponent<PlayfabPlayer>();
        Debug.Log(loginManager.playerData.ToJson());
    }

    public void OnPageActive()
    {
        setupDisplay();
    }

    private void setupDisplay()
    {
        Debug.Log(speed);
        Debug.Log(loginManager.playerData.stats.maxSpeed);
        speed.text = loginManager.playerData.stats.maxSpeed.ToString();
        sprint.text = loginManager.playerData.powerUps.sprintTicket.ToString();
        stamina.text = loginManager.playerData.stats.minStamina.ToString();
        marathon.text = loginManager.playerData.powerUps.marathonTicket.ToString();
        width.text = loginManager.playerData.stats.width.ToString();
        food.text = loginManager.playerData.powerUps.foodCoupon.ToString();
        height.text = loginManager.playerData.stats.height.ToString();
        milk.text = loginManager.playerData.powerUps.milkCoupon.ToString();
        exchange.text = $"Reset ({loginManager.playerData.powerUps.exchangeProgram})";
    }

    public void addSpeed()
    {
        if (!loginManager.hasLogin) return;

        if (loginManager.playerData.powerUps.sprintTicket <= 0) return;
        loginManager.playerData.stats.maxSpeed += 1;
        loginManager.playerData.powerUps.sprintTicket -= 1;
        speed.text = loginManager.playerData.stats.maxSpeed.ToString();
        sprint.text = loginManager.playerData.powerUps.sprintTicket.ToString();
    }
    public void addStamina()
    {
        if (!loginManager.hasLogin) return;

        if (loginManager.playerData.powerUps.marathonTicket <= 0) return;
        loginManager.playerData.stats.minStamina += 1;
        loginManager.playerData.powerUps.marathonTicket -= 1;
        stamina.text = loginManager.playerData.stats.minStamina.ToString();
        marathon.text = loginManager.playerData.powerUps.marathonTicket.ToString();
    }
    public void addWidth()
    {
        if (!loginManager.hasLogin) return;

        if (loginManager.playerData.powerUps.foodCoupon <= 0) return;
        loginManager.playerData.stats.width += 1;
        loginManager.playerData.powerUps.foodCoupon -= 1;
        width.text = loginManager.playerData.stats.width.ToString();
        food.text = loginManager.playerData.powerUps.foodCoupon.ToString();
    }
    public void addHeight()
    {
        if (!loginManager.hasLogin) return;

        if (loginManager.playerData.powerUps.milkCoupon <= 0) return;
        loginManager.playerData.stats.height+= 1;
        loginManager.playerData.powerUps.milkCoupon -= 1;
        height.text = loginManager.playerData.stats.height.ToString();
        milk.text = loginManager.playerData.powerUps.milkCoupon.ToString();
    }

    public void save()
    {
        if (!loginManager.hasLogin) return;
        playerManager.SendPlayerData(loginManager.playerData);
    }

    public void reset()
    {
        if (!loginManager.hasLogin) return;

        if (loginManager.playerData.powerUps.exchangeProgram <= 0) return;
        loginManager.playerData.powerUps.exchangeProgram -= 1;

        loginManager.playerData.powerUps.sprintTicket += loginManager.playerData.stats.maxSpeed - 1;
        loginManager.playerData.powerUps.marathonTicket += loginManager.playerData.stats.minStamina - 1;
        loginManager.playerData.powerUps.foodCoupon += loginManager.playerData.stats.width - 1;
        loginManager.playerData.powerUps.milkCoupon += loginManager.playerData.stats.height - 1;
        loginManager.playerData.stats.maxSpeed = 1;
        loginManager.playerData.stats.minStamina = 1;
        loginManager.playerData.stats.width = 1;
        loginManager.playerData.stats.height = 1;

        setupDisplay();
    }

}
