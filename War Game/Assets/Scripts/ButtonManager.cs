using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField]
    public Button Back_ss_btn;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Back_ss_btn.onClick.AddListener(WelcomeScene);
        }
        catch (System.NullReferenceException)
        {
            Debug.Log("Some buttons are missing from the scene because they are not required");
        }
    }

    void WelcomeScene()
    {
        SceneManager.LoadScene("WelcomeScene");
    }
}