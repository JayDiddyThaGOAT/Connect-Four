using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : Singleton<GameManager>
{    
    private Connect4Board connect4Board;

    private InputManager inputManager;

    private const byte RESET_GAME = 7;

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
            PhotonNetwork.Disconnect();
        }
        else
            SceneManager.LoadScene("StartMenu");
    }

    public void SetResetButtonVisible(bool visible)
    {
        Button resetButton = GameObject.FindGameObjectWithTag("Reset Button").GetComponent<Button>();
        resetButton.interactable = visible;
    }

    private IEnumerator StartResettingGame()
    {
        AudioSource resetButtonAudioSource = GameObject.FindGameObjectWithTag("Reset Button").GetComponent<AudioSource>();
        resetButtonAudioSource.Play();

        while (resetButtonAudioSource.isPlaying)
            yield return null;

        SetResetButtonVisible(false);

        if (!PhotonNetwork.IsConnected)
        {
            connect4Board.ResetBoard();
            StartCoroutine(connect4Board.RotateBoard());
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "Gameplay")
            {
                SetResetButtonVisible(false);

                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{{"Ready To Play?", true}});

                int opponentPlayerIndex = PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[0] ? 1 : 0; 
                Player opponentPlayer = PhotonNetwork.PlayerList[opponentPlayerIndex];

                Transform canvas = GameObject.Find("Canvas").transform;
                GameObject loadingPanel = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/LoadingPanel"), canvas);

                TMP_Text loadingPanelText = loadingPanel.transform.Find("LoadingUpdateText").GetComponent<TMP_Text>();
                loadingPanelText.text = $"Waiting for {opponentPlayer.NickName}";

                bool isOpponentReadyToPlay = opponentPlayer.CustomProperties.ContainsKey("Ready To Play?") && (bool)opponentPlayer.CustomProperties["Ready To Play?"];
                while (!isOpponentReadyToPlay)
                    yield return null;
                
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions{Receivers=ReceiverGroup.All};
                PhotonNetwork.RaiseEvent(RESET_GAME, null, raiseEventOptions, SendOptions.SendReliable);
            }
            else
            {
                connect4Board.ResetBoard();
                StartCoroutine(connect4Board.RotateBoard());
            }
        }
    }

    public void ResetGame()
    {
        if (connect4Board.IsBlackDiscAI && connect4Board.IsWhiteDiscAI)
        {
            connect4Board.ResetBoard();
            StartCoroutine(connect4Board.RotateBoard());
        }
        else
            StartCoroutine("StartResettingGame");
    }

    void OnEventReceived(EventData obj)
    {
        if (obj.Code == RESET_GAME)
        {
            GameObject loadingPanel = GameObject.FindGameObjectWithTag("Loading Panel");

            TMP_Text loadingPanelText = loadingPanel.transform.Find("LoadingUpdateText").GetComponent<TMP_Text>();
            loadingPanelText.text = "Resetting Game";
            
            Destroy(loadingPanel);

            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{{"Ready To Play?", null}});

            inputManager = InputManager.Instance;
            inputManager.enabled = (Connect4Player)PhotonNetwork.LocalPlayer.CustomProperties["Disc Color"] == connect4Board.WinnerPlayer;

            connect4Board.ResetBoard();
            StartCoroutine(connect4Board.RotateBoard());
        }
    }
}
