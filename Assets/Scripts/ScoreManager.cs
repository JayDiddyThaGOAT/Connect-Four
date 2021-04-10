using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class ScoreManager : Singleton<ScoreManager>
{
#pragma warning disable 0649
    [SerializeField]
    private TMP_Text BlackDiscScoreText;

    [SerializeField]
    private TMP_Text WhiteDiscScoreText;

    [SerializeField]
    private GameObject TieText;

    [SerializeField]
    private int StartBlackDiscScore = 0;

    [SerializeField]
    private int StartWhiteDiscScore = 0;
#pragma warning restore 0649

    void Start()
    {
        PlayerPrefs.SetInt("Black Disc Score", StartBlackDiscScore);
        PlayerPrefs.SetInt("White Disc Score", StartWhiteDiscScore);

        SetBlackDiscScore(PlayerPrefs.GetInt("Black Disc Score"));
        SetWhiteDiscScore(PlayerPrefs.GetInt("White Disc Score"));
    }

    public void SetBlackDiscScore(int score)
    {
        PlayerPrefs.SetInt("Black Disc Score", score);
        BlackDiscScoreText.text = score.ToString();
    }

    public void SetWhiteDiscScore(int score)
    {
        PlayerPrefs.SetInt("White Disc Score", score);
        WhiteDiscScoreText.text = score.ToString();
    }

    public void SetTieTextActive(bool active)
    {
        TieText.SetActive(active);
    }
}
