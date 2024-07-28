using System;
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
    public GameObject AccDamageUI;
    TextMeshProUGUI accText;
    TextMeshProUGUI dmgText;
    public TextMeshProUGUI healText;
    public GameObject CharacterUIs;

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

        AccDamageUI = GameObject.Find("AccDamageUI");
        accText = AccDamageUI.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        dmgText = AccDamageUI.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
        AccDamageUI.SetActive(false);

        healText = GameObject.Find("HealUI").GetComponentInChildren<TextMeshProUGUI>();
        healText.transform.parent.gameObject.SetActive(false);

        CharacterUIs = GameObject.Find("CharacterUIs");
    }

    void Update()
    {
        // 임시
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (CharacterUIs.activeSelf)
            {
                MiniStatue.transform.parent.gameObject.SetActive(false);
                CharacterUIs.SetActive(false);
            }
            else
            {
                CharacterUIs.SetActive(true);
            }
        }
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

    // 기술 장착 바꿀 때도 확인
    public void ShowAccDmgInfo(Hero hero, EnemyAI enemy)
    {
        if (hero.equippedTech.TechType != TechType.Heal && hero.equippedTech.TechType != TechType.StressHeal)
        {
            float acc = hero.equippedTech.Acc + hero.characterData.AccMod - enemy.characterData.Dodge;
            accText.SetText(string.Format("명중률: {0}%", acc));

            int minDamage;
            int maxDamage;

            if (!hero.equippedTech.isFixedDamage)
            {
                minDamage
                = Mathf.RoundToInt(hero.characterData.minDamage * (hero.equippedTech.dMGMod == Mod.positive ? (1 + hero.equippedTech.DamageMod) : (1 - hero.equippedTech.DamageMod)));
                minDamage = Mathf.RoundToInt(minDamage * ((100 - enemy.characterData.defense) / 100));
                maxDamage
                = Mathf.RoundToInt(hero.characterData.maxDamage * (hero.equippedTech.dMGMod == Mod.positive ? (1 + hero.equippedTech.DamageMod) : (1 - hero.equippedTech.DamageMod)));
                maxDamage = Mathf.RoundToInt(maxDamage * ((100 - enemy.characterData.defense) / 100));
            }
            else
            {
                minDamage = Mathf.RoundToInt(hero.equippedTech.FixedMinDamage * ((100 - enemy.characterData.defense) / 100));
                maxDamage = Mathf.RoundToInt(hero.equippedTech.FixedMaxDamage * ((100 - enemy.characterData.defense) / 100));
            }
            dmgText.SetText(string.Format("데미지 범위: {0}~{1}", minDamage, maxDamage));

            AccDamageUI.SetActive(true);
            AccDamageUI.transform.position = enemy.transform.position + new Vector3(-4, 0, 0);
        }
    }

    public void HideAccDmgInfo()
    {
        AccDamageUI.SetActive(false);
    }

    public void ShowHealInfo(Hero hero)
    {
        float minDamage;
        float maxDamage;

        if (!hero.equippedTech.isFixedDamage)
        {
            minDamage
            = Mathf.RoundToInt(hero.characterData.minDamage * (hero.equippedTech.dMGMod == Mod.positive ? (1 + hero.equippedTech.DamageMod) : (1 - hero.equippedTech.DamageMod)));
            maxDamage
            = Mathf.RoundToInt(hero.characterData.maxDamage * (hero.equippedTech.dMGMod == Mod.positive ? (1 + hero.equippedTech.DamageMod) : (1 - hero.equippedTech.DamageMod)));
        }
        else
        {
            minDamage = hero.equippedTech.FixedMinDamage;
            maxDamage = hero.equippedTech.FixedMaxDamage;
        }

        if (hero.equippedTech.TechType == TechType.Heal)
            healText.SetText(string.Format("힐: {0}~{1}", minDamage, maxDamage));
        else if (hero.equippedTech.TechType == TechType.StressHeal)
            healText.SetText(string.Format("스트레스 힐: {0}~{1}", minDamage, maxDamage));

        healText.transform.parent.gameObject.SetActive(true);
        healText.transform.parent.position = hero.transform.position + new Vector3(-4, 0, 0);
    }

    public void HideHealInfo()
    {
        healText.transform.parent.gameObject.SetActive(false);
    }
}
