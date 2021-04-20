using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

using Photon.Pun;
using Photon.Realtime;

public class StartMenuManager : Singleton<StartMenuManager>
{    
    private const string READY_TO_PLAY = "Ready To Play?";

#pragma warning disable 0649
    [SerializeField]
    private RectTransform StartMenuButtonPanel;

    [SerializeField]
    private RectTransform DiscSelectionPanel;

     [SerializeField]
    private Button QuickGameButton;

    [SerializeField]
    private Button MatchmakingButton;

    [SerializeField]
    private Button SinglePlayerButton;

    [SerializeField]
    private Button BlackDiscButton;

    [SerializeField]
    private Button WhiteDiscButton;

    [SerializeField]
    private Button BackButton;

    [SerializeField]
    private Button PlayButton;

    [SerializeField]
    private Image BlackDiscPointer;

    [SerializeField]
    private Image WhiteDiscPointer;

    [SerializeField]
    private TMP_Text BlackDiscPlayerName;

    [SerializeField]
    private TMP_Text WhiteDiscPlayerName;

    [SerializeField]
    private TMP_Text InstructionsText;

    [SerializeField]
    private float PanelShiftDuration = 0.25f;

    private NetworkManager networkManager;

    private Button _selectedDiscButton;

    [HideInInspector]
    public Button SelectedDiscButton{
        get{return _selectedDiscButton;}
        set{_selectedDiscButton = value;}
    }

    float StartMenuButtonPanelWidth, DiscSelectionPanelWidth;

#pragma warning restore 0649

    private Connect4Board connect4Board;

    void Start()
    {
        networkManager = GetComponent<NetworkManager>();

        connect4Board = Connect4Board.Instance;
        
        StartMenuButtonPanelWidth = StartMenuButtonPanel.GetComponent<RectTransform>().sizeDelta.x;
        DiscSelectionPanelWidth = DiscSelectionPanel.GetComponent<RectTransform>().sizeDelta.x;
    }

    void SetConnect4BoardAIs(bool blackAI, bool whiteAI)
    {
        connect4Board.SetBlackDiscAI(blackAI);
        connect4Board.SetWhiteDiscAI(whiteAI);
    }

    private IEnumerator LoadDiscSelectionPanel()
    {
        float t = 0;
        while (t <= PanelShiftDuration)
        {
            t += Time.deltaTime;

            float StartMenuButtonPanelPositionX = Mathf.Lerp(0, -StartMenuButtonPanelWidth - 140, t / PanelShiftDuration);
            StartMenuButtonPanel.localPosition = new Vector3(StartMenuButtonPanelPositionX, StartMenuButtonPanel.localPosition.y, StartMenuButtonPanel.localPosition.z);

            float DiscSelectionPanelPositionX = Mathf.Lerp(DiscSelectionPanelWidth, 0, t / PanelShiftDuration);
            DiscSelectionPanel.localPosition = new Vector3(DiscSelectionPanelPositionX, DiscSelectionPanel.localPosition.y, DiscSelectionPanel.localPosition.z);

            yield return null;
        }

        BackButton.interactable = true;
    }

    private IEnumerator LoadStartMenuButtonPanel()
    {
        BackButton.interactable = false;
        PlayButton.interactable = false;

        BlackDiscPointer.enabled = false;
        WhiteDiscPointer.enabled = false;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();

            while (PhotonNetwork.IsConnected)
                yield return null;
        }

        float t = 0;
        while (t <= PanelShiftDuration)
        {
            t += Time.deltaTime;

            float StartMenuButtonPanelPositionX = Mathf.Lerp(-StartMenuButtonPanelWidth - 140, 0, t / PanelShiftDuration);
            StartMenuButtonPanel.localPosition = new Vector3(StartMenuButtonPanelPositionX, StartMenuButtonPanel.localPosition.y, StartMenuButtonPanel.localPosition.z);

            float DiscSelectionPanelPositionX = Mathf.Lerp(0, DiscSelectionPanelWidth, t / PanelShiftDuration);
            DiscSelectionPanel.localPosition = new Vector3(DiscSelectionPanelPositionX, DiscSelectionPanel.localPosition.y, DiscSelectionPanel.localPosition.z);

            yield return null;
        }
        
        SetQuickGameButtonInteractable(true);
        SetSinglePlayerButtonInteractable(true);

    }

    public void GoToDiscSelectionPanel()
    {
        StartCoroutine("LoadDiscSelectionPanel");
    }

    public void GoToStartMenuButtonPanel()
    {
        StartCoroutine("LoadStartMenuButtonPanel");
    }

    public void SetDiscButtonInteractable(int discColor, bool interactable)
    {
        if (discColor == (int)Connect4Player.None)
            return;

        Button discButton = discColor == (int)Connect4Player.Black ? BlackDiscButton : WhiteDiscButton;
        discButton.interactable = interactable;
    }
    
    public void SetPlayButtonInteractable(bool interactable)
    {
        PlayButton.interactable = interactable;
    }

     public void SetQuickGameButtonInteractable(bool interactable)
    {
        QuickGameButton.interactable = interactable;
    }

     public void SetMatchmakingButtonInteractable(bool interactable)
    {
        MatchmakingButton.interactable = interactable;
    }

     public void SetSinglePlayerButtonInteractable(bool interactable)
    {
        SinglePlayerButton.interactable = interactable;
    }

    [PunRPC]
    public void SetSelectedDiscButton(int discColor)
    {
        if (discColor == (int)Connect4Player.None)
            return;

        SelectedDiscButton = discColor == (int)Connect4Player.Black ? BlackDiscButton : WhiteDiscButton;
    }

    [PunRPC]
    public void SetPlayerName(int discColor, string text)
    {
        if (discColor == (int)Connect4Player.None)
            return;
        
        TMP_Text playerName = discColor == (int)Connect4Player.Black ? BlackDiscPlayerName : WhiteDiscPlayerName;
        playerName.text = text;
    }

    [PunRPC]
    public void TogglePlayerNameVisibility(int discColor)
    {
        if (discColor == (int)Connect4Player.None)
            return;
        
        TMP_Text playerName = discColor == (int)Connect4Player.Black ? BlackDiscPlayerName : WhiteDiscPlayerName;
        playerName.enabled ^= true;
    }

    [PunRPC]
    public void SetInstructionsText(string text)
    {
        InstructionsText.text = text;
    }

    [PunRPC]
    public void PlayGameRPC()
    {
        SetConnect4BoardAIs(false, false);

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("Gameplay");
    }

    public void SelectDisc(int discColor)
    {
        if (discColor == (int)Connect4Player.None)
            return;

        TMP_Text playerName = discColor == (int)Connect4Player.Black ? BlackDiscPlayerName : WhiteDiscPlayerName;

        Button otherDiscButton = discColor == (int)Connect4Player.Black ? WhiteDiscButton : BlackDiscButton;


        if (PhotonNetwork.IsConnected)
        {
            networkManager.photonView.RPC("TogglePlayerNameVisibility", RpcTarget.All, discColor);
            networkManager.photonView.RPC("SetSelectedDiscButton", RpcTarget.All, discColor);
            networkManager.photonView.RPC("SetPlayerName", RpcTarget.All, discColor, PhotonNetwork.LocalPlayer.NickName);

            networkManager.SelectDisc(playerName.enabled ? discColor : 0);
           
            if (PhotonNetwork.PlayerList.Length == 1)
                otherDiscButton.interactable = !playerName.enabled;
            else
            {
                int localPlayerIndex = PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[0] ? 0 : 1;

                int opponentPlayerIndex = localPlayerIndex == 0 ? 1 : 0;
                Player opponentPlayer = PhotonNetwork.PlayerList[opponentPlayerIndex];
                Connect4Player opponentPlayerColor = (Connect4Player)opponentPlayer.CustomProperties["Disc Color"];

                otherDiscButton.interactable = !playerName.enabled && opponentPlayerColor == Connect4Player.None;
            }
        }
        else
        {
            Image discPointer = discColor == (int)Connect4Player.Black ? BlackDiscPointer : WhiteDiscPointer;
            discPointer.enabled ^= true;

            otherDiscButton.interactable = !discPointer.enabled;

            SetInstructionsText(discPointer.enabled ? "Tap Play" : "Tap A Disc");
            SetPlayButtonInteractable(discPointer.enabled);
        }
    }

    public void SetPlayerNameVisiblity(int discColor, bool visible)
    {
        if ((Connect4Player)discColor == Connect4Player.None)
            return;

        TMP_Text playerName = discColor == (int)Connect4Player.Black ? BlackDiscPlayerName : WhiteDiscPlayerName;
        playerName.enabled = visible;
    }

    public void PlayGame()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SetConnect4BoardAIs(!BlackDiscPointer.enabled, !WhiteDiscPointer.enabled);
            SceneManager.LoadScene("Gameplay");
        }
        else
        {
            SetPlayButtonInteractable(false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{{READY_TO_PLAY, true}});
            
            int opponentPlayerIndex = PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[0] ? 1 : 0; 
            Player opponentPlayer = PhotonNetwork.PlayerList[opponentPlayerIndex];
            
            if (opponentPlayer.CustomProperties.ContainsKey(READY_TO_PLAY))
               networkManager.photonView.RPC("PlayGameRPC", RpcTarget.All);
        }
    }
}
