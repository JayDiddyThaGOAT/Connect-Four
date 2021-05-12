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
    private Button OnlineGameButton;

    [SerializeField]
    private Button LocalGameButton;

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
    private TMP_InputField usernameInputField;

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

    float PanelSlideDistance = 2000.0f;

#pragma warning restore 0649

    void Awake()
    {
        if (Application.isEditor)
            PlayerPrefs.DeleteAll();

        networkManager = GetComponent<NetworkManager>();
    }

    void OnEnable()
    {
        if (PlayerPrefs.HasKey("Username"))
        {
            AudioSource inputFieldAudioSource = usernameInputField.GetComponent<AudioSource>();
            inputFieldAudioSource.volume = 0.0f;

            usernameInputField.text = PlayerPrefs.GetString("Username");
            usernameInputField.textComponent.text = string.Empty;

            SetOnlineGameButtonInteractable(usernameInputField.text.Length > 0);
        }
        else
            SetOnlineGameButtonInteractable(false);
        
        usernameInputField.onValueChanged.AddListener((username) => SetUsername(username));
    }

    void SetConnect4BoardAIs(bool blackAI, bool whiteAI)
    {
        Connect4Board board = Connect4Board.Instance;
        board.SetBlackDiscAI(blackAI);
        board.SetWhiteDiscAI(whiteAI);
    }

    private IEnumerator LoadDiscSelectionPanel()
    {
        if (!PhotonNetwork.IsConnected)
        {
            AudioSource localGameButtonAudioSource = LocalGameButton.GetComponent<AudioSource>();
            localGameButtonAudioSource.Play();

            while (localGameButtonAudioSource.isPlaying)
                yield return null;
        }

        float t = 0;
        while (t <= PanelShiftDuration)
        {
            t += Time.deltaTime;

            float StartMenuButtonPanelPositionX = Mathf.Lerp(0, -PanelSlideDistance, t / PanelShiftDuration);
            StartMenuButtonPanel.localPosition = new Vector3(StartMenuButtonPanelPositionX, StartMenuButtonPanel.localPosition.y, StartMenuButtonPanel.localPosition.z);

            float DiscSelectionPanelPositionX = Mathf.Lerp(PanelSlideDistance, 0, t / PanelShiftDuration);
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

        AudioSource backButtonAudioSource = BackButton.GetComponent<AudioSource>();
        backButtonAudioSource.Play();

        float t = 0;
        while (t <= PanelShiftDuration)
        {
            t += Time.deltaTime;

            float StartMenuButtonPanelPositionX = Mathf.Lerp(-PanelSlideDistance, 0, t / PanelShiftDuration);
            StartMenuButtonPanel.localPosition = new Vector3(StartMenuButtonPanelPositionX, StartMenuButtonPanel.localPosition.y, StartMenuButtonPanel.localPosition.z);

            float DiscSelectionPanelPositionX = Mathf.Lerp(0, PanelSlideDistance, t / PanelShiftDuration);
            DiscSelectionPanel.localPosition = new Vector3(DiscSelectionPanelPositionX, DiscSelectionPanel.localPosition.y, DiscSelectionPanel.localPosition.z);

            yield return null;
        }

        BlackDiscButton.interactable = true;
        WhiteDiscButton.interactable = true;
        SetInstructionsText("Tap A Disc");
        
        SetUserNameInputFieldInteractable(true);
        SetOnlineGameButtonInteractable(true);
        SetLocalGameButtonInteractable(true);

    }

    private IEnumerator LoadGameplayScene()
    {
        SetPlayButtonInteractable(false);

        AudioSource playGameAudioSource = PlayButton.GetComponent<AudioSource>();
        playGameAudioSource.Play();

        while (playGameAudioSource.isPlaying)
            yield return null;

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Gameplay");
        }
        else
        {
            SetConnect4BoardAIs(!BlackDiscPointer.enabled, !WhiteDiscPointer.enabled);
            SceneManager.LoadScene("Gameplay");
        }
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

        if (discButton != null)
            discButton.interactable = interactable;
    }
    
    public void SetPlayButtonInteractable(bool interactable)
    {
        PlayButton.interactable = interactable;
    }

    public void SetOnlineGameButtonInteractable(bool interactable)
    {
        OnlineGameButton.interactable = interactable;
    }

    public void PlayOnlineGameButtonSound()
    {
        AudioSource onlineGameButtonAudioSource = OnlineGameButton.GetComponent<AudioSource>();
        onlineGameButtonAudioSource.Play();
    }

    public void SetLocalGameButtonInteractable(bool interactable)
    {
        LocalGameButton.interactable = interactable;
    }

    public void SetUserNameInputFieldInteractable(bool interactable)
    {
        usernameInputField.interactable = interactable;
    }

    public void SetUsername(string input)
    {
        AudioSource inputFieldAudioSource = usernameInputField.GetComponent<AudioSource>();
        inputFieldAudioSource.volume = 0.5f;
        inputFieldAudioSource.Play();
        
        SetOnlineGameButtonInteractable(input.Length > 0);
        PlayerPrefs.SetString("Username", input);
    }

    [PunRPC]
    public void SetSelectedDiscButton(int discColor)
    {
        if (discColor == (int)Connect4Player.None)
            return;

        SelectedDiscButton = discColor == (int)Connect4Player.Black ? BlackDiscButton : WhiteDiscButton;

        AudioSource discButtonAudioSource = SelectedDiscButton.GetComponent<AudioSource>();
        discButtonAudioSource.Play();
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
        StartCoroutine("LoadGameplayScene");
    }

    public void SelectDisc(int discColor)
    {
        Debug.Assert(discColor != (int)Connect4Player.None);

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
            AudioSource discButtonAudioSource;
            if (discColor == (int)Connect4Player.Black)
                discButtonAudioSource = BlackDiscButton.GetComponent<AudioSource>();
            else
                discButtonAudioSource = WhiteDiscButton.GetComponent<AudioSource>();
            discButtonAudioSource.Play();

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

        if (playerName != null)
            playerName.enabled = visible;
    }

    public void PlayGame()
    {
        if (!PhotonNetwork.IsConnected)
        {
            StartCoroutine("LoadGameplayScene");
        }
        else
        {
            SetPlayButtonInteractable(false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable{{READY_TO_PLAY, true}});
            
            int opponentPlayerIndex = PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[0] ? 1 : 0; 
            Player opponentPlayer = PhotonNetwork.PlayerList[opponentPlayerIndex];
            
            if (opponentPlayer.CustomProperties.ContainsKey(READY_TO_PLAY) && (bool)opponentPlayer.CustomProperties[READY_TO_PLAY])
               networkManager.photonView.RPC("PlayGameRPC", RpcTarget.All);
            else
            {
                AudioSource playGameAudioSource = PlayButton.GetComponent<AudioSource>();
                playGameAudioSource.Play();
            }
        }
    }
}
