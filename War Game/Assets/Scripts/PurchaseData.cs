using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseData
{
    public string playerId;
    public string itemPurchased;
    public string date;
    public string time;

    public PurchaseData(string playerId, string itemPurchased, string date, string time)
    {
        this.playerId = playerId;
        this.itemPurchased = itemPurchased; 
        this.date = date;
        this.time = time;
    }
}
