using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }

    // Buttons
    public void OnLocalGameButton()
    {
        Debug.Log("Local Game");
    }
    public void OnOnlineGameButton()
    {
        Debug.Log("Online Game");
    }

    public void OnOnlineHostButton()
    {
        Debug.Log("Host");
    }
    public void OnOnlineConnectButton()
    {
        Debug.Log("Connect");
    }
    public void OnOnlineBackButton()
    {
        Debug.Log("Back From Online");
    }

    public void OnHostBackButton()
    {
        Debug.Log("Back From Host");
    }
}
