using UnityEngine;
using TMPro;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance;

    [SerializeField] private TMP_Text coinsText;
    private int coins;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Start()
    {
        // Initialize coins from PlayerPrefs or set default value
        coins = PlayerPrefs.GetInt("PlayerCoins", 1000);
        UpdateCoinsText();
    }

    public void UpdateCoinsText()
    {
        coinsText.text = "Coins: " + coins;
    }

    public bool DeductCoins(int amount)
    {
        // Check if the player has enough coins to cover the deduction
        if (coins >= amount)
        {
            // Deduct the specified amount from the player's coins
            coins -= amount;
            // Update the displayed coins text in the UI
            UpdateCoinsText();
            return true;
        }
        else
        {
            return false; 
        }
    }
}