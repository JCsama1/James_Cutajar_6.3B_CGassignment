using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Storage;
using Firebase.Extensions;
using JetBrains.Annotations;
using TMPro;

public class FirebaseStorageController : MonoBehaviour
{
    private FirebaseStorage _firebaseInstance;
    [SerializeField] private GameObject ThumbnailPrefab;
    private GameObject _thumbnailContainer;
    public List<GameObject> instantiatedPrefabs;
    public List<AssetData> DownloadedAssetData;
    public enum DownloadType { Manifest, Thumbnail }
    private WalletManager walletManager;

    public static FirebaseStorageController Instance { get; private set; }

    private void Awake()
    {
        //Singleton Pattern
        if (Instance != this && Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this); // GameManager
        _firebaseInstance = FirebaseStorage.DefaultInstance;
    }

    private void Start()
    {
        instantiatedPrefabs = new List<GameObject>();
        _thumbnailContainer = GameObject.Find("Thumbnail_Container");

        // Get reference to WalletManager
        walletManager = WalletManager.Instance;

        // Display default coins in the top right corner
        DisplayPlayerWallet();

        // First download manifest.txt
        DownloadFileAsync("gs://dlc-store-assignment.appspot.com/manifest.xml", DownloadType.Manifest);
        // Get the urls inside the manifest file
        // Download each url and display to the user
    }

    private void DisplayPlayerWallet()
    {
        // Call UpdateCoinsText to display the default coins
        walletManager.UpdateCoinsText();
    }

    public void DownloadFileAsync(string url, DownloadType filetype, [Optional] AssetData assetRef)
    {
        StorageReference storageRef = _firebaseInstance.GetReferenceFromUrl(url);

        // Download in memory with a maximum allowed size of 1MB (1 * 1024 * 1024 bytes)
        const long maxAllowedSize = 1 * 1024 * 1024 * 4;
        storageRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogException(task.Exception);
                // Uh-oh, an error occurred!
            }
            else
            {
                Debug.Log($"{storageRef.Name} finished downloading!");
                if (filetype == DownloadType.Manifest)
                {
                    // Load manifest
                    StartCoroutine(LoadManifest(task.Result));
                }
                else if (filetype == DownloadType.Thumbnail)
                {
                    // Load the image into Unity
                    StartCoroutine(LoadImageContainer(task.Result, assetRef));
                }
            }
        });
    }

    IEnumerator LoadManifest(byte[] byteArr)
    {
        XDocument manifest = XDocument.Parse(System.Text.Encoding.UTF8.GetString(byteArr));
        DownloadedAssetData = new List<AssetData>();

        foreach (XElement assetElement in manifest.Root.Elements("asset"))
        {
            string itemId = assetElement.Element("itemId")?.Value;
            string itemDescription = assetElement.Element("itemDescription")?.Value;
            string previewImageUrl = assetElement.Element("previewImageUrl")?.Element("url")?.Value;
            string priceStr = assetElement.Element("itemPrice")?.Element("value")?.Value;
            float price = (priceStr != null) ? float.Parse(priceStr) : 0.0f;

            AssetData newAsset = new AssetData(itemId, itemDescription, previewImageUrl, price);
            DownloadedAssetData.Add(newAsset);
            DownloadFileAsync(newAsset.PreviewImageUrl, DownloadType.Thumbnail, newAsset);
        }

        yield return null;
    }

    IEnumerator LoadImageContainer(byte[] byteArr, AssetData assetRef)
{
    Texture2D imageTex = new Texture2D(1, 1);
    imageTex.LoadImage(byteArr);

    // Instantiate a new prefab
    GameObject thumbnailPrefab =
        Instantiate(ThumbnailPrefab, _thumbnailContainer.transform.position,
            Quaternion.identity, _thumbnailContainer.transform);
    thumbnailPrefab.name = "Thumbnail_" + instantiatedPrefabs.Count;

    // Get the BuyButton script from the Buy_Button GameObject
    BuyButton buyButton = thumbnailPrefab.transform.GetChild(3).GetComponent<BuyButton>();
    
    // Set the AssetData for the BuyButton script
    buyButton.SetAssetData(assetRef);

    // Load the image to that prefab
    thumbnailPrefab.transform.GetChild(0).GetComponent<RawImage>().texture = imageTex;
    thumbnailPrefab.transform.GetChild(1).GetComponent<TMP_Text>().text = assetRef.ItemDescription;
    thumbnailPrefab.transform.GetChild(2).GetComponent<TMP_Text>().text = assetRef.ItemPrice + " Coins";

    instantiatedPrefabs.Add(thumbnailPrefab);
    yield return null;
}
}