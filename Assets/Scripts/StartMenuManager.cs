using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    private RectTransform DiscSelector;

    [SerializeField]
    private GameObject DiscSelectionText;

    [SerializeField]
    private float PanelShiftDuration = 0.25f;
#pragma warning restore 0649

    private Connect4Board connect4Board;

    private RectTransform BlackDiscButtonTransform;

    private RectTransform WhiteDiscButtonTransform;

    void Start()
    {
        connect4Board = Connect4Board.Instance;
        
        BlackDiscButtonTransform = BlackDiscButton.GetComponent<RectTransform>();
        WhiteDiscButtonTransform = WhiteDiscButton.GetComponent<RectTransform>();
    }

    private IEnumerator LoadDiscSelectionPanel()
    {
        float t = 0;
        while (t <= PanelShiftDuration)
        {
            t += Time.deltaTime;

            float StartMenuButtonPanelPositionX = Mathf.Lerp(0, -1440, t / PanelShiftDuration);
            StartMenuButtonPanel.localPosition = new Vector3(StartMenuButtonPanelPositionX, StartMenuButtonPanel.localPosition.y, StartMenuButtonPanel.localPosition.z);

            float DiscSelectionPanelPositionX = Mathf.Lerp(1440, 0, t / PanelShiftDuration);
            DiscSelectionPanel.localPosition = new Vector3(DiscSelectionPanelPositionX, DiscSelectionPanel.localPosition.y, DiscSelectionPanel.localPosition.z);

            yield return null;
        }

        BackButton.interactable = true;
    }

    private IEnumerator LoadStartMenuButtonPanel()
    {
        BackButton.interactable = false;

        float t = 0;
        while (t <= PanelShiftDuration)
        {
            t += Time.deltaTime;

            float StartMenuButtonPanelPositionX = Mathf.Lerp(-1440, 0, t / PanelShiftDuration);
            StartMenuButtonPanel.localPosition = new Vector3(StartMenuButtonPanelPositionX, StartMenuButtonPanel.localPosition.y, StartMenuButtonPanel.localPosition.z);

            float DiscSelectionPanelPositionX = Mathf.Lerp(0, 1440, t / PanelShiftDuration);
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
        if (DiscSelector.parent == WhiteDiscButtonTransform)
            return;

        DiscSelector.SetParent(WhiteDiscButtonTransform, false);
        DiscSelector.localPosition = new Vector3(DiscSelector.localPosition.x + 24.0f, DiscSelector.localPosition.y, DiscSelector.localPosition.z);
        DiscSelector.GetComponent<Image>().color = Color.white;
    }

    public void SelectBlackDisc()
    {
        if (DiscSelector.parent == BlackDiscButtonTransform)
            return;

        DiscSelector.SetParent(BlackDiscButtonTransform, false);
        DiscSelector.localPosition = new Vector3(DiscSelector.localPosition.x - 24.0f, DiscSelector.localPosition.y, DiscSelector.localPosition.z);
        DiscSelector.GetComponent<Image>().color = Color.black;
    }

    public void PlayGame()
    {
        connect4Board.SetBlackDiscAI(DiscSelector.parent != BlackDiscButtonTransform);
        connect4Board.SetWhiteDiscAI(DiscSelector.parent != WhiteDiscButtonTransform);

        SceneManager.LoadScene("Gameplay");
    }
}
