using TMPro;
using Firebase;
using Firebase.Database;
using UnityEngine;

public class BuyButton : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemPriceText;

    private AssetData assetData;

    public void SetAssetData(AssetData data)
    {
        assetData = data;
        itemNameText.text = data.ItemDescription;
        itemPriceText.text = data.ItemPrice.ToString() + " Coins";
    }

    public void OnBuyButtonClick()
    {
        if (WalletManager.Instance.DeductCoins((int)assetData.ItemPrice))
        {
            // Player has enough coins, unlock the item
            // You can add your unlock logic here
            Debug.Log("Item purchased: " + assetData.ItemDescription);

            // Save purchase information to Firebase Realtime Database
            SavePurchaseToDatabase(assetData.ItemId, assetData.ItemDescription);
        }
        else
        {
            Debug.Log("Not enough coins to purchase: " + assetData.ItemDescription);
        }
    }

    private void SavePurchaseToDatabase(string itemId, string itemDescription)
{
    // Get a reference to your Firebase Realtime Database
    DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

    // Generate a random unique player ID
    string playerId = System.Guid.NewGuid().ToString();

    // Get the current date and time
    string date = System.DateTime.Now.ToString("yyyy-MM-dd");
    string time = System.DateTime.Now.ToString("HH:mm:ss");

    // Create a PurchaseData object
    PurchaseData purchaseData = new PurchaseData(playerId, itemDescription, date, time);

    // Convert the PurchaseData object to JSON
    string json = JsonUtility.ToJson(purchaseData);

    // Save the JSON data to the database under a unique key
    string key = databaseReference.Child("purchases").Push().Key;
    databaseReference.Child("purchases").Child(key).SetRawJsonValueAsync(json);
}
}