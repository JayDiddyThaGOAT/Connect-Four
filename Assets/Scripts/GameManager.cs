using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    private Connect4Board connect4Board;

    void Start()
    {
        connect4Board = Connect4Board.Instance;
    }

    public void GoBackToStartMenu()
    {
        connect4Board.SetBlackDiscAI(true);
        connect4Board.SetWhiteDiscAI(true);
        
        SceneManager.LoadScene("StartMenu");
    }
}
