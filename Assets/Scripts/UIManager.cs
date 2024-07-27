using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : SingleTon<UIManager>
{
    [Header("오브젝트")]
    TextMeshProUGUI GameInfo;

    [Header("프리팹")]
    public TextMeshProUGUI CombatInfo;

    List<string> combatInfos = new List<string>();

    void Awake()
    {
        GameInfo = GameObject.Find("GameInfo").GetComponentInChildren<TextMeshProUGUI>();
        GameInfo.transform.parent.gameObject.SetActive(false);
    }

    public void ShowGameInfo(string text)
    {
        GameInfo.SetText(text);
        GameInfo.transform.parent.gameObject.SetActive(true);
    }

    public void HideGameInfo()
    {
        GameInfo.transform.parent.gameObject.SetActive(false);
    }
}