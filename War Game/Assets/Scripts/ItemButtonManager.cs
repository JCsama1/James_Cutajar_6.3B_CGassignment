using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemButtonManager : MonoBehaviour
{
    private Transform thumbnailPrefab;
    private Transform locked;
    private Transform unlocked;
    private Transform price;
    private Transform name;
    private Transform progressBar;
    private Transform progressFill;

    void Start()
    {
        thumbnailPrefab = transform.parent;
        locked = thumbnailPrefab.GetChild(0);
        unlocked = thumbnailPrefab.GetChild(1);
        price = thumbnailPrefab.GetChild(4);
        name = thumbnailPrefab.GetChild(3);
        progressBar = thumbnailPrefab.GetChild(7);
        progressFill = progressBar.GetChild(0);
    }
    
    public void handleBuy()
    {
        int priceValue = int.Parse(price.GetComponent<TMP_Text>().text);
        //handle price
        //price is deducted from balance after content is downloaded
        if (priceValue <= WalletManager.Instance.coins)
        {
            //handle border
            unlocked.GetComponent<RawImage>().enabled = true;
            locked.GetComponent<RawImage>().enabled = false;
            
            //handle content download
            progressBar.GetComponent<Image>().enabled = true;
            progressFill.GetComponent<Image>().enabled = true;
            FirebaseStorageController.Instance.downloadContent(name.GetComponent<TMP_Text>().text, progressBar, progressFill);
            
            //handle button style
            gameObject.GetComponent<Button>().interactable = false;
            transform.GetChild(0).GetComponent<TMP_Text>().text = "OWNED";
        }
    }
}
