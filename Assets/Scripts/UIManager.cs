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
        CombatInfo = GameObject.Find("CombatInfo").GetComponent<TextMeshProUGUI>();
        GameInfo = GameObject.Find("GameInfo").GetComponentInChildren<TextMeshProUGUI>();
        CombatInfo.gameObject.SetActive(false);
        GameInfo.transform.parent.gameObject.SetActive(false);
    }

    public void AddCombatInfo(string text)
    {
        combatInfos.Add(text);
    }
    public void AddCombatInfo(string text, int index)
    {
        combatInfos.Insert(index, text);
    }

    public void ShowCombatInfos(Vector3 position)
    {
        StartCoroutine(readCombatInfos(position));
    }

    IEnumerator readCombatInfos(Vector3 position)
    {
        CombatInfo.gameObject.SetActive(true);
        CombatInfo.transform.position = position;

        foreach (string text in combatInfos)
        {
            CombatInfo.SetText(text);

            yield return new WaitForSeconds(1f);
        }

        CombatInfo.gameObject.SetActive(false);
        combatInfos.Clear();

        // 카메라 줌아웃
        CombatManager.Instance.CallZoomOutCamera();
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