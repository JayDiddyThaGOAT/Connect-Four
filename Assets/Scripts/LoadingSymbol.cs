using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSymbol : MonoBehaviour
{
    [SerializeField]
    private float moveColorWaitTime = 0.05f;

    private Image [] loadingKnobs;

    void Awake()
    {
        loadingKnobs = GetComponentsInChildren<Image>();
    }

    void OnEnable()
    {
        StartCoroutine("Loading");
    }

    void OnDisable()
    {
        StopAllCoroutines();

        for (int i = 0; i < loadingKnobs.Length; i++)
        {
            loadingKnobs[i].color = new Color(i / 8.0f, i / 8.0f, i / 8.0f);
        }
    }

    private IEnumerator Loading()
    {
        while (true)
        {
            Color firstKnobColor = loadingKnobs[0].color;
            for (int i = 0; i < loadingKnobs.Length - 1; i++)
            {
                loadingKnobs[i].color = loadingKnobs[i + 1].color;
            }

            loadingKnobs[loadingKnobs.Length - 1].color = firstKnobColor;

            yield return new WaitForSeconds(moveColorWaitTime);
        }
    }
}
