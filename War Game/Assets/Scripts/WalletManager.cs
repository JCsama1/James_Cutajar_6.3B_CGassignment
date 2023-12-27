using UnityEngine;
using TMPro;

public class WalletManager : MonoBehaviour
{
    private int coins;
    public TMP_Text coinsLabel; // Reference to the Coins_Label TextMeshPro

    public static WalletManager Instance;

    public int Coins
    {
        get { return coins; }
        set
        {
            coins = value;
            PlayerPrefs.SetInt("Coins", coins);
            //UpdateUI(); // Call a method to update the UI with the new wallet amount
        }
    }

    private void Awake()
{
    Debug.Log("WalletManager Awake");

    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize coins from PlayerPrefs or set a default value
        coins = PlayerPrefs.GetInt("Coins", 1000);

        // Update the UI on awake
        UpdateUI();

        Debug.Log("WalletManager initialized with coins: " + coins);
    }
}

    private void UpdateUI()
{
    Debug.Log("Updating UI with coins: " + coins);
    
    // Update the UI to display the current wallet amount
    if (coinsLabel != null)
    {
        coinsLabel.text = "Coins: " + coins.ToString();
    }
}
}
