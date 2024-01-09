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

    // Add the DeductCoins method to subtract coins
    public bool DeductCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateCoinsText();
            return true; // Deduction successful
        }
        else
        {
            return false; // Insufficient funds
        }
    }
}