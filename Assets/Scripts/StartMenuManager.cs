using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

public class StartMenuManager : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private RectTransform StartMenuButtonPanel;

    [SerializeField]
    private RectTransform DiscSelectionPanel;

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
    private TMP_Text InstructionsText;

    [SerializeField]
    private float PanelShiftDuration = 0.25f;

    float StartMenuButtonPanelWidth, DiscSelectionPanelWidth;

#pragma warning restore 0649

    private Connect4Board connect4Board;

    private RectTransform BlackDiscButtonTransform;

    private RectTransform WhiteDiscButtonTransform;

    void Start()
    {
        connect4Board = Connect4Board.Instance;
        
        BlackDiscButtonTransform = BlackDiscButton.GetComponent<RectTransform>();
        WhiteDiscButtonTransform = WhiteDiscButton.GetComponent<RectTransform>();

        StartMenuButtonPanelWidth = StartMenuButtonPanel.GetComponent<RectTransform>().sizeDelta.x;
        DiscSelectionPanelWidth = DiscSelectionPanel.GetComponent<RectTransform>().sizeDelta.x;
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
    }

    public void GoToDiscSelectionPanel()
    {
        StartCoroutine("LoadDiscSelectionPanel");
    }

     public void GoToStartMenuButtonPanel()
    {
        StartCoroutine("LoadStartMenuButtonPanel");
    }
    
    public void SelectWhiteDisc()
    {
        WhiteDiscPointer.enabled = true;
        BlackDiscPointer.enabled = false;

        PlayButton.interactable = true;
    }

    public void SelectBlackDisc()
    {
        BlackDiscPointer.enabled = true;
        WhiteDiscPointer.enabled = false;

        PlayButton.interactable = true;
    }

    public void PlayGame()
    {
        connect4Board.SetBlackDiscAI(!BlackDiscPointer.enabled);
        connect4Board.SetWhiteDiscAI(!WhiteDiscPointer.enabled);

        SceneManager.LoadScene("Gameplay");
    }
}
