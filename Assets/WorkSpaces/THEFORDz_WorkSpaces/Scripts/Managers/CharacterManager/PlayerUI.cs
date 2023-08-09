using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Netcode;
using Unity.VisualScripting;

public class PlayerUI : NetworkBehaviour
{
    [Serializable]
    public struct HealthUI
    {
        public GameObject healthUI;
        public Image playerIconImage;
        public TextMeshProUGUI playerIdText;
        public TextMeshProUGUI playerNameText;
        public Slider healthSlider;
        public Image healthImage;
        public HealthColor healthColor;
        public TextMeshProUGUI scoreText;
    }

    [Serializable]
    public struct DeathUI
    {
        public GameObject deathUI;
        public Image playerIconDeathImage;
        public TextMeshProUGUI playerIdDeathText;
    }
    
    [SerializeField]
    HealthUI m_healthUI;                // A struct for all the data relate to the health UI

    [SerializeField]
    DeathUI m_deathUI;                  // A struct for all the data relate to the death UI

    [Header("Set in runtime")]
    public int maxHealth;               // Max health the player has, use for the conversion to the

    [ClientRpc]
    void UpdateHealthClientRpc(float currentHealth)
    {
        if (IsServer)
            return;
        
        m_healthUI.healthSlider.value = currentHealth;
        m_healthUI.healthImage.color = m_healthUI.healthColor.GetHealthColor(currentHealth);

        if (currentHealth <= 0f)
        {
            // Turn off lifeUI
            m_healthUI.healthUI.SetActive(false);

            // Turn on deathUI
            m_deathUI.deathUI.SetActive(true);
        }
    }

    [ClientRpc]
    void UpdateScoreClientRpc(int score)
    {
        m_healthUI.scoreText.text = string.Format(""+score);
    }
    public void SetUI(
        int playerId,
        string playerName,
        Sprite playerIcon,
        Sprite playerDeathIcon,
        int maxHealth,
        Color color,int score)
    {
        m_healthUI.playerIconImage.sprite = playerIcon;
        m_healthUI.playerIdText.color = color;
        m_healthUI.playerIdText.text = $"Player{(playerId + 1)}";
        m_healthUI.playerNameText.text = playerName;

        m_deathUI.playerIdDeathText.color = color;
        m_deathUI.playerIconDeathImage.sprite = playerDeathIcon;
        m_healthUI.scoreText.text = string.Format(""+score);
        
        this.maxHealth = maxHealth;
        m_healthUI.healthImage.color = m_healthUI.healthColor.normalColor;
        
        // Turn on my lifeUI
        m_healthUI.healthUI.SetActive(true);

        // Safety -> inactive in scene
        m_deathUI.deathUI.SetActive(false);
    }

    // Update the UI health 
    public void UpdateHealth(int currentHealth)
    {
        if (!IsServer)
            return;

        // Don't let health to go below 
        currentHealth = currentHealth < 0 ? 0 : currentHealth;

        float convertedHealth = (float)currentHealth / (float)maxHealth;
        m_healthUI.healthSlider.value = convertedHealth;
        m_healthUI.healthImage.color = m_healthUI.healthColor.GetHealthColor(convertedHealth);

        if (currentHealth <= 0)
        {
            // Turn off lifeUI
            m_healthUI.healthUI.SetActive(false);

            // Turn on deathUI
            m_deathUI.deathUI.SetActive(true);
        }

        UpdateHealthClientRpc(convertedHealth);
    }

    public void UpdateScore(int currentScore)
    {

        UpdateScoreClientRpc(currentScore);
    }

}
