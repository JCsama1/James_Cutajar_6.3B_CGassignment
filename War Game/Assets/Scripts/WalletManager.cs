using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public int coins = 1000;
    private GameObject wallet;
    
    public static WalletManager Instance
    {
        get;
        private set;
    }
    private void Awake()
    {
        //Singleton Pattern
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateWallet()
    {
        wallet = GameObject.Find("Wallet");
        wallet.GetComponent<TMP_Text>().text = "Coins: " + coins;
    }
}