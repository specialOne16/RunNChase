using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataManager : MonoBehaviour
{
    private PlayerData playerData;

    public Text speed;
    public Text stamina;
    public Text width;
    public Text height;

    public Text sprint;
    public Text marathon;
    public Text food;
    public Text milk;

    public Text exchange;

    private void Start()
    {
        setupPlayerData();
        setupDisplay();
    }

    private void setupPlayerData()
    {
        // TODO : ganti dengan data dari playfab
        playerData = new PlayerData();
        playerData.powerUps.sprintTicket = 4;
        playerData.powerUps.marathonTicket = 3;
        playerData.powerUps.foodCoupon = 5;
        playerData.powerUps.milkCoupon = 4;
        playerData.powerUps.exchangeProgram = 2;
    }

    private void setupDisplay()
    {
        speed.text = playerData.stats.maxSpeed.ToString();
        sprint.text = playerData.powerUps.sprintTicket.ToString();
        stamina.text = playerData.stats.minStamina.ToString();
        marathon.text = playerData.powerUps.marathonTicket.ToString();
        width.text = playerData.stats.width.ToString();
        food.text = playerData.powerUps.foodCoupon.ToString();
        height.text = playerData.stats.height.ToString();
        milk.text = playerData.powerUps.milkCoupon.ToString();
        exchange.text = $"Reset ({playerData.powerUps.exchangeProgram})";
    }

    public void addSpeed()
    {
        if (playerData.powerUps.sprintTicket <= 0) return;
        playerData.stats.maxSpeed += 1;
        playerData.powerUps.sprintTicket -= 1;
        speed.text = playerData.stats.maxSpeed.ToString();
        sprint.text = playerData.powerUps.sprintTicket.ToString();
    }
    public void addStamina()
    {
        if (playerData.powerUps.marathonTicket <= 0) return;
        playerData.stats.minStamina += 1;
        playerData.powerUps.marathonTicket -= 1;
        stamina.text = playerData.stats.minStamina.ToString();
        marathon.text = playerData.powerUps.marathonTicket.ToString();
    }
    public void addWidth()
    {
        if (playerData.powerUps.foodCoupon <= 0) return;
        playerData.stats.width += 1;
        playerData.powerUps.foodCoupon -= 1;
        width.text = playerData.stats.width.ToString();
        food.text = playerData.powerUps.foodCoupon.ToString();
    }
    public void addHeight()
    {
        if (playerData.powerUps.milkCoupon <= 0) return;
        playerData.stats.height+= 1;
        playerData.powerUps.milkCoupon -= 1;
        height.text = playerData.stats.height.ToString();
        milk.text = playerData.powerUps.milkCoupon.ToString();
    }

    public void save()
    {
        Debug.Log("Saved");
        // TODO : Save data ke playfab
    }

    public void reset()
    {
        if (playerData.powerUps.exchangeProgram <= 0) return;
        playerData.powerUps.exchangeProgram -= 1;

        playerData.powerUps.sprintTicket += playerData.stats.maxSpeed - 1;
        playerData.powerUps.marathonTicket += playerData.stats.minStamina - 1;
        playerData.powerUps.foodCoupon += playerData.stats.width - 1;
        playerData.powerUps.milkCoupon += playerData.stats.height - 1;
        playerData.stats.maxSpeed = 1;
        playerData.stats.minStamina = 1;
        playerData.stats.width = 1;
        playerData.stats.height = 1;

        setupDisplay();
    }

}
