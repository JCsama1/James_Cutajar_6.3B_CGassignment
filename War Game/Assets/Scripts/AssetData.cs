using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetData
{
    private string _itemId;
    private string _itemDescription;
    private string _previewImageUrl;
    private float _itemPrice;

    // Constructor to initialize the asset data with provided values
    public AssetData(string itemId, string itemDescription, string previewImageUrl, float itemPrice)
    {
        // Set the values for the asset data
        ItemId = itemId;
        ItemDescription = itemDescription;
        PreviewImageUrl = previewImageUrl;
        ItemPrice = itemPrice;
    }

    public string ItemId
    {
        get => _itemId;
        set => _itemId = value;
    }

    public string ItemDescription
    {
        get => _itemDescription;
        set => _itemDescription = value;
    }

    public string PreviewImageUrl
    {
        get => _previewImageUrl;
        set => _previewImageUrl = value;
    }

    public float ItemPrice
    {
        get => _itemPrice;
        set => _itemPrice = value;
    }

    public override string ToString()
    {
        return $"Asset with itemId: {ItemId}, description: {ItemDescription}, " +
               $"previewUrl: {PreviewImageUrl}, price: {ItemPrice}";
    }
}