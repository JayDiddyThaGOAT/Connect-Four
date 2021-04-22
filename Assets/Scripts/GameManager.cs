using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : Singleton<GameManager>
{    
    private Connect4Board connect4Board;

    private InputManager inputManager;

    private const byte RESET_GAME = 7;

    private const byte LEAVE_GAME = 8;

#pragma warning disable 0649
    [SerializeField]
    private Button resetButton;
#pragma warning restore 0649

    void Start()
    {
        connect4Board = Connect4Board.Instance;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
            inputManager = InputManager.Instance;
        }
    }

    public void GoBackToStartMenu()
    {
        if (PhotonNetwork.IsConnected)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions{Receivers=ReceiverGroup.All};
            PhotonNetwork.RaiseEvent(LEAVE_GAME, null, raiseEventOptions, SendOptions.SendReliable);
        }
        else
            SceneManager.LoadScene("StartMenu");
    }

    public void SetResetButtonActive(bool active)
    {
        resetButton.gameObject.SetActive(active);
        resetButton.interactable = active;
    }

    public void ResetGame()
    {
        if (!PhotonNetwork.IsConnected)
        {
            connect4Board.ResetBoard();
            StartCoroutine(connect4Board.RotateBoard());
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "Gameplay")
            {
                resetButton.interactable = false;

                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{{"Ready To Play?", true}});

                int opponentPlayerIndex = PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[0] ? 1 : 0; 
                Player opponentPlayer = PhotonNetwork.PlayerList[opponentPlayerIndex];
                
                if (opponentPlayer.CustomProperties.ContainsKey("Ready To Play?") && (bool)opponentPlayer.CustomProperties["Ready To Play?"])
                {
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions{Receivers=ReceiverGroup.All};
                    PhotonNetwork.RaiseEvent(RESET_GAME, null, raiseEventOptions, SendOptions.SendReliable);
                }
            }
            else
            {
                connect4Board.ResetBoard();
                StartCoroutine(connect4Board.RotateBoard());
            }
        }
    }

    void OnEventReceived(EventData obj)
    {
        if (obj.Code == RESET_GAME)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{{"Ready To Play?", null}});

            inputManager = InputManager.Instance;
            inputManager.enabled = (Connect4Player)PhotonNetwork.LocalPlayer.CustomProperties["Disc Color"] == connect4Board.WinnerPlayer;

            connect4Board.ResetBoard();
            StartCoroutine(connect4Board.RotateBoard());
        }
        else if (obj.Code == LEAVE_GAME)
        {
            PhotonNetwork.Disconnect();
        }
    }
}
