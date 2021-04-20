using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;

public class GameManager : Singleton<GameManager>
{    
    private Connect4Board connect4Board;
    void Start()
    {
        connect4Board = Connect4Board.Instance;
    }

    public void GoBackToStartMenu()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
        
        SceneManager.LoadScene("StartMenu");
    }
}
