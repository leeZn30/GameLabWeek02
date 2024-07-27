using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : SingleTon<UIManager>
{
    [Header("오브젝트")]
    TextMeshProUGUI GameInfo;
    public TextMeshProUGUI MiniStatue;
    public GameObject Statue;
    public TextMeshProUGUI Accuracy;

    [Header("프리팹")]
    public TextMeshProUGUI CombatInfo;

    void Awake()
    {
        GameInfo = GameObject.Find("GameInfo").GetComponentInChildren<TextMeshProUGUI>();
        GameInfo.transform.parent.gameObject.SetActive(false);

        MiniStatue = GameObject.Find("MiniStateUI").GetComponentInChildren<TextMeshProUGUI>();
        MiniStatue.transform.parent.gameObject.SetActive(false);

        Statue = GameObject.Find("StateUI");
        Statue.SetActive(false);

        Accuracy = GameObject.Find("AccuracyUI").GetComponentInChildren<TextMeshProUGUI>();
        Accuracy.transform.parent.gameObject.SetActive(false);
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

    public void ShowAccInfo(Hero hero, EnemyAI enemy)
    {
        float acc = hero.equippedTech.Acc + hero.characterData.AccMod - enemy.characterData.Dodge;
        Accuracy.SetText(string.Format("명중률: {0}%", acc));

        if (!Accuracy.transform.parent.gameObject.activeSelf)
        {
            Accuracy.transform.parent.gameObject.SetActive(true);
        }

        Accuracy.transform.parent.position = enemy.transform.position + new Vector3(0, 3, 0);
    }
}