using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;

using UnityEngine;
using UnityEngine.UI;

using Firebase.Storage;
using Firebase.Extensions;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.VersionControl;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Task = System.Threading.Tasks.Task;

public class FirebaseStorageController : MonoBehaviour
{
    private FirebaseStorage _firebaseInstance;
    [SerializeField] private GameObject ThumbnailPrefab;
    private GameObject _thumbnailContainer;
    public List<GameObject> instantiatedPrefabs;
    public List<AssetData> DownloadedAssetData;
    private XDocument manifest;
    
    public GameObject buySpecialEffect;

    public enum DownloadType
    {
        Manifest, Thumbnail
    }

    public enum GameItemType
    {
        Background
    }

    public static FirebaseStorageController Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        // Singleton Pattern
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
        _firebaseInstance = FirebaseStorage.DefaultInstance;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += prepScene;
    }

    private void prepScene(Scene scene, LoadSceneMode mode)
    {
        if (scene == SceneManager.GetSceneByName("StoreScene") && mode == LoadSceneMode.Single)
        {
            prepStore();
        }
    }

    public void prepStore()
    {
        WalletManager.Instance.updateWallet();
        instantiatedPrefabs = new List<GameObject>();
        _thumbnailContainer = GameObject.Find("Thumbnail_Container");

        if (DownloadedAssetData == null)
        {
            DownloadFileAsync("gs://dlc-store-assignment.appspot.com/manifest.xml", DownloadType.Manifest);
        }
        else
        {
            foreach (AssetData asset in DownloadedAssetData)
            {
                DownloadFileAsync(asset.ThumbnailUrl, DownloadType.Thumbnail, asset);
            }
        }
    }

    public void DownloadFileAsync(string url, DownloadType filetype, [Optional] AssetData assetRef){
        StorageReference storageRef =  _firebaseInstance.GetReferenceFromUrl(url);
        
        // Download in memory with a maximum allowed size of 32MB (32 * 1024 * 1024 bytes)
        const long maxAllowedSize = 32 * 1024 * 1024;
        storageRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogException(task.Exception);
                Debug.LogError($"Download failed for URL: {url}");
            }
            else
            {
                Debug.Log($"{storageRef.Name} finished downloading successfully!");

                if (filetype == DownloadType.Manifest)
                {
                    StartCoroutine(LoadManifest(task.Result));
                }
                else if (filetype == DownloadType.Thumbnail)
                {
                    StartCoroutine(LoadImageContainer(task.Result, assetRef));
                }
            }
        });
    }

    IEnumerator LoadManifest(byte[] byteArr)
    {
        manifest = XDocument.Parse(System.Text.Encoding.UTF8.GetString(byteArr));
        DownloadedAssetData = new List<AssetData>();
        //Get the urls inside the manifest file
        //Download each url and display to the user
        foreach (XElement xElement in manifest.Root.Elements())
        {
            string itemId = xElement.Element("itemId")?.Value;
            string name = xElement.Element("name")?.Value;
            string thumbnailUrl = xElement.Element("img")?.Element("url")?.Value;
            string priceStr = xElement.Element("price")?.Element("value")?.Value;
            int price = (priceStr != null) ? int.Parse(priceStr) : 0;
            bool owned = false;
            AssetData newAsset = new AssetData(itemId, name, thumbnailUrl, price, owned);
            DownloadedAssetData.Add(newAsset);
            DownloadFileAsync(newAsset.ThumbnailUrl, DownloadType.Thumbnail, newAsset);
        }
        yield return null;
    }

    IEnumerator LoadImageContainer(byte[] byteArr, AssetData assetRef)
    {
        Texture2D imageTex = new Texture2D(1, 1);
        imageTex.LoadImage(byteArr);
        //Instantiate a new prefab
        GameObject thumbnailPrefab =
            Instantiate(ThumbnailPrefab, _thumbnailContainer.transform.position, 
                Quaternion.identity,_thumbnailContainer.transform);
        thumbnailPrefab.name = "Thumbnail_" + instantiatedPrefabs.Count;
        //Load the image to that prefab
        thumbnailPrefab.transform.GetChild(2).GetComponent<RawImage>().texture = imageTex;
        thumbnailPrefab.transform.GetChild(3).GetComponent<TMP_Text>().text = assetRef.Name;
        thumbnailPrefab.transform.GetChild(4).GetComponent<TMP_Text>().text = assetRef.Price.ToString();

        instantiatedPrefabs.Add(thumbnailPrefab);
        
        //loaded for each item loaded into the scene (4 times)
        checkIfItemsAreAffordable(assetRef);
        checkIfItemsAreOwned(assetRef);
        yield return null;
    }

    public void checkIfItemsAreAffordable(AssetData assetRef)
    {
        if (!assetRef.Owned)
        {
            //goes through store items, makes ref to the below
            foreach (GameObject nameLabel in GameObject.FindGameObjectsWithTag("ItemName"))
            {   
                // and finds the one matching the current assetRef
                if (nameLabel.GetComponent<TMP_Text>().text == assetRef.Name)
                {
                    //ref parent container
                    Transform itemContainer = nameLabel.transform.parent;
                    //ref price object
                    Transform priceObject = itemContainer.GetChild(4);
                    //ref text of button
                    TMP_Text btnTextComponent = itemContainer.GetChild(6).GetChild(0).GetComponent<TMP_Text>();
                    //parses to integer so it can compare to emojicoin count to see if its larger and also check if the text of the button is set to "BUY"
                    //the text of the button turns red only if the price is higher than the wallet value and if the item is locked
                    if (int.Parse(priceObject.GetComponent<TMP_Text>().text) > WalletManager.Instance.coins)
                    {
                        btnTextComponent.color = Color.red;
                    }
                    else
                    {
                        btnTextComponent.color = Color.black;
                    }
                }
            }
        }
    }

    public void checkIfItemsAreOwned(AssetData assetRef)
    {
        // checks if items are owned and then styles appropriately
        if (assetRef.Owned)
        {
            // goes through all name labels
            foreach (GameObject nameLabel in GameObject.FindGameObjectsWithTag("ItemName"))
            {   
                // and finds the one matching the current assetRef
                if (nameLabel.GetComponent<TMP_Text>().text == assetRef.Name)
                {
                    //ref the elements
                    Transform itemContainer = nameLabel.transform.parent;
                    
                    Transform unlocked = itemContainer.GetChild(1);
                    Transform locked = itemContainer.GetChild(0);
                    Transform itemButton = itemContainer.GetChild(6);

                    //style the container
                    locked.GetComponent<RawImage>().enabled = false;
                    
                    unlocked.GetComponent<RawImage>().enabled = true;
                    
                    itemButton.GetComponent<Button>().interactable = false;
                    itemButton.GetChild(0).GetComponent<TMP_Text>().text = "OWNED";
                }
            }

        }
    }

    public void downloadContent(string itemName, Transform progressBar, Transform progressFill)
    {
        int itemID = -1;
        switch (itemName)
        {
            case "Background 1":
                itemID = 0;
                break;

            case "Background 2":
                itemID = 1;
                break;

            case "Background 3":
                itemID = 2;
                break;

            default:
                break;
        }
        string contentUrl = manifest.Root.Elements()?.ElementAt(itemID)?.Element("content")?.Element("url")?.Value;

        print(contentUrl.Substring(54));
        StorageReference storageRef =  _firebaseInstance.GetReferenceFromUrl(contentUrl);   
        
        // Create local filesystem URL
        string localUrl = Application.streamingAssetsPath + "/Content/" + contentUrl.Substring(54);
        //string localUrl = Application.streamingAssetsPath + "/Content/" + itemUrl;

        // Download to the local filesystem
        Task task = storageRef.GetFileAsync(localUrl, new StorageProgress<DownloadState>(state => {
            // called periodically during the download
            /*Debug.Log(String.Format(
                "Progress: {0} of {1} bytes transferred.",
                state.BytesTransferred,
                state.TotalByteCount
            ));*/
            
            //progress bar filling up
            new WaitForEndOfFrame();
            progressFill.GetComponent<RectTransform>().localScale = new Vector3((float)state.BytesTransferred/(float)state.TotalByteCount, progressFill.localScale.y, progressFill.localScale.z);

        }), CancellationToken.None);

        task.ContinueWithOnMainThread(resultTask => {
            if (!task.IsFaulted && !task.IsCanceled) {
                Debug.Log("File downloaded." + Application.streamingAssetsPath);
                //Debug.Log("File downloaded." + Application.streamingAssetsPath);
                
                // find relevant asset and set to owned
                foreach (AssetData asset in DownloadedAssetData)
                {
                    if (itemName == asset.Name)
                    {
                        
                        //change wallet balance
                        WalletManager.Instance.coins -= asset.Price;
                    }
                    
                    //update wallet display
                    WalletManager.Instance.updateWallet();
                    
                    checkIfItemsAreAffordable(asset);
                }
                
                // do special effect
                Instantiate(buySpecialEffect, progressBar.position, Quaternion.identity);
                
                // disable progress bar
                progressBar.GetComponent<Image>().enabled = false;
                progressFill.GetComponent<Image>().enabled = false;
            }
            else
            {
                Debug.LogException(task.Exception);
                Debug.LogError($"Download failed for content: {itemName}");
            }
        });
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= prepScene;
    }
}