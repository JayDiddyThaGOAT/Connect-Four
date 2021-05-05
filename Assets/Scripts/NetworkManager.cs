using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private const string DISC_COLOR = "Disc Color";

    private const string READY_TO_PLAY = "Ready To Play?";

    private const int MAX_PLAYERS = 2;

    private StartMenuManager startMenuManager;

    private Connect4Board connect4Board;

    void Start()
    {
        PhotonNetwork.KeepAliveInBackground = 30;

        if (SceneManager.GetActiveScene().name == "StartMenu")
            startMenuManager = StartMenuManager.Instance;
    }

    void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void ConnectToQuickGame()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Can't connect to the internet");
            return;
        }

        if (startMenuManager != null)
        {
            startMenuManager.SetUserNameInputFieldInteractable(false);
            startMenuManager.SetOnlineGameButtonInteractable(false);
            startMenuManager.SetLocalGameButtonInteractable(false);
        }

        if (PhotonNetwork.IsConnected)
        {
            Debug.Log($"Connected to server. Looking for random room");
            PhotonNetwork.JoinRandomRoom(null, MAX_PLAYERS);
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    #region Photon Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log($"Connected to server. Looking for random room");
        PhotonNetwork.JoinRandomRoom(null, MAX_PLAYERS);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"Joining random room failed b/c {message}. Creating new room");
        PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = MAX_PLAYERS });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined the room");

        if (PlayerPrefs.HasKey("Username"))
            PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("Username");
        else
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
                PhotonNetwork.LocalPlayer.NickName = "Player 1";
            else
                PhotonNetwork.LocalPlayer.NickName = "Player 2";
        }
        

        PrepareDiscChoices();

        if (startMenuManager != null)
            startMenuManager.GoToDiscSelectionPanel();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.ActorNumber} entered the room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.ActorNumber} left the room");

        var otherPlayerDiscColor = (Connect4Player)otherPlayer.CustomProperties[DISC_COLOR];
        var localPlayerDiscColor = (Connect4Player)PhotonNetwork.LocalPlayer.CustomProperties[DISC_COLOR];

        if (startMenuManager != null)
        {
            startMenuManager.SetPlayerNameVisiblity((int)otherPlayerDiscColor, false);

            if (localPlayerDiscColor == Connect4Player.None)
            {
                startMenuManager.SetInstructionsText("Tap A Disc");

                if (otherPlayerDiscColor == Connect4Player.Black)
                    startMenuManager.SetDiscButtonInteractable((int)Connect4Player.Black, true);
                else if (otherPlayerDiscColor == Connect4Player.White)
                    startMenuManager.SetDiscButtonInteractable((int)Connect4Player.White, true);
            }
            else
            {
                startMenuManager.SetInstructionsText("Finding 2<sup>nd</sup> Player");
            }

            startMenuManager.SetPlayButtonInteractable(false);
        }
        else
            PhotonNetwork.Disconnect();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (startMenuManager == null)
            return;
        
        if (changedProps.ContainsKey(DISC_COLOR))
        {
            var targetPlayerDiscColor = (Connect4Player)changedProps[DISC_COLOR];

            if (targetPlayerDiscColor != Connect4Player.None)
                Debug.Log($"Player {targetPlayer.ActorNumber} selected {targetPlayerDiscColor} Disc");
            else
                Debug.Log($"Player {targetPlayer.ActorNumber} selected No Disc");


            if (targetPlayer != PhotonNetwork.LocalPlayer)
            {
                Connect4Player localPlayerColor = (Connect4Player)PhotonNetwork.LocalPlayer.CustomProperties[DISC_COLOR];

                if (targetPlayerDiscColor == Connect4Player.Black)
                    startMenuManager.SetDiscButtonInteractable((int)Connect4Player.Black, false);
                else if (targetPlayerDiscColor == Connect4Player.White)
                    startMenuManager.SetDiscButtonInteractable((int)Connect4Player.White, false);
                else if (targetPlayerDiscColor == Connect4Player.None)
                {
                    if (startMenuManager.SelectedDiscButton == null)
                        return;

                    if (startMenuManager.SelectedDiscButton.name == "BlackDiscButton")
                    {
                        if (localPlayerColor == Connect4Player.None)
                            startMenuManager.SetDiscButtonInteractable((int)Connect4Player.Black, true);
                    }
                    else if (startMenuManager.SelectedDiscButton.name == "WhiteDiscButton")
                    {
                        if (localPlayerColor == Connect4Player.None)
                            startMenuManager.SetDiscButtonInteractable((int)Connect4Player.White, true);
                    }
                }
            }

            if (PhotonNetwork.PlayerList.Length > 1)
            {
                Player firstPlayer = PhotonNetwork.PlayerList[0];
                Player secondPlayer = PhotonNetwork.PlayerList[1];

                Connect4Player firstPlayerColor = (Connect4Player)firstPlayer.CustomProperties[DISC_COLOR];
                Connect4Player secondPlayerColor = (Connect4Player)secondPlayer.CustomProperties[DISC_COLOR];
                
                startMenuManager.SetPlayButtonInteractable(firstPlayerColor != Connect4Player.None && secondPlayerColor != Connect4Player.None);

                if (firstPlayerColor != Connect4Player.None && secondPlayerColor != Connect4Player.None)
                    startMenuManager.SetInstructionsText("Tap Play");
                else if (firstPlayerColor == Connect4Player.None && secondPlayerColor == Connect4Player.None)
                    startMenuManager.SetInstructionsText("Tap A Disc");
                else
                {
                    if (firstPlayerColor == Connect4Player.None)
                        startMenuManager.SetInstructionsText("Wait For " + firstPlayer.NickName);
                    else if (secondPlayerColor == Connect4Player.None)
                        startMenuManager.SetInstructionsText("Wait For " + secondPlayer.NickName);
                }
            }
            else
            {
                if (targetPlayerDiscColor != Connect4Player.None)
                    startMenuManager.SetInstructionsText("Finding 2<sup>nd</sup> Player");
                else
                    startMenuManager.SetInstructionsText("Tap A Disc");
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Player {newMasterClient.ActorNumber} is the new host");

        if (startMenuManager != null)
        {
            if (newMasterClient.CustomProperties.ContainsKey(DISC_COLOR))
            {
                newMasterClient.NickName = "Player 1";

                Connect4Player newMasterClientColor = (Connect4Player)newMasterClient.CustomProperties[DISC_COLOR];
                startMenuManager.SetPlayerName((int)newMasterClientColor, newMasterClient.NickName);
            }

            startMenuManager.SetPlayButtonInteractable(false);
        }
        else
        {
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Leaving the server");

        if (startMenuManager != null)
        {
            startMenuManager.SetInstructionsText("Tap A Disc");

            startMenuManager.SetDiscButtonInteractable((int)Connect4Player.Black, true);
            startMenuManager.SetDiscButtonInteractable((int)Connect4Player.White, true);

            startMenuManager.SetPlayerNameVisiblity((int)Connect4Player.Black, false);
            startMenuManager.SetPlayerNameVisiblity((int)Connect4Player.White, false);

            startMenuManager.SetPlayButtonInteractable(false);
        }
        else
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{{"Ready To Play?", null}});
            PhotonNetwork.LoadLevel("StartMenu");
        }
    }

    #endregion

    [PunRPC]
    internal void SelectDisc(int discColor)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{
            {
                DISC_COLOR, discColor
            }
        });
    }

    void PrepareDiscChoices()
    {
        if (startMenuManager == null)
            return;

        SelectDisc(0);

        startMenuManager.SetPlayButtonInteractable(false);

        if (PhotonNetwork.PlayerList.Length > 1)
        {
            Player firstPlayer = PhotonNetwork.PlayerList[0];
            if (firstPlayer.CustomProperties.ContainsKey(DISC_COLOR))
            {
                Connect4Player firstPlayerDiscColor = (Connect4Player)firstPlayer.CustomProperties[DISC_COLOR];
                
                if (firstPlayerDiscColor != (int)Connect4Player.None)
                {
                    startMenuManager.SetPlayerNameVisiblity((int)firstPlayerDiscColor, true);

                    Player secondPlayer = PhotonNetwork.PlayerList[1];
                    startMenuManager.SetInstructionsText("Wait For " + secondPlayer.NickName);
                }

                if (firstPlayerDiscColor == Connect4Player.Black)
                {
                    startMenuManager.SetPlayerName((int)Connect4Player.Black, firstPlayer.NickName);
                    startMenuManager.SetDiscButtonInteractable((int)Connect4Player.Black, false);
                }
                else if (firstPlayerDiscColor == Connect4Player.White)
                {
                    startMenuManager.SetPlayerName((int)Connect4Player.White, firstPlayer.NickName);
                    startMenuManager.SetDiscButtonInteractable((int)Connect4Player.White, false);
                }
            }
        }
    }
}