using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StateUI : MonoBehaviour
{
    [Header("캐릭터 능력치")]
    [SerializeField] TextMeshProUGUI characterNameText;
    [SerializeField] TextMeshProUGUI stepText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI defenseText;
    [SerializeField] TextMeshProUGUI dodgeText;
    [SerializeField] TextMeshProUGUI stunResistText;
    [SerializeField] TextMeshProUGUI bleedResistText;
    [SerializeField] TextMeshProUGUI poisonResistText;

    [Header("기술 능력치")]
    [SerializeField] TextMeshProUGUI skillIndexText;
    [SerializeField] TextMeshProUGUI skillNameText;
    [SerializeField] TextMeshProUGUI skillTypeText;
    [SerializeField] TextMeshProUGUI skillTargetText;
    [SerializeField] TextMeshProUGUI skillCriticalText;
    [SerializeField] TextMeshProUGUI stunText;
    [SerializeField] TextMeshProUGUI bleedText;
    [SerializeField] TextMeshProUGUI bleedDescText;
    [SerializeField] TextMeshProUGUI poisonText;
    [SerializeField] TextMeshProUGUI poisonDescText;

    public void ShowStat(Character character)
    {
        CharacterData data = character.characterData;

        characterNameText.SetText(data.ID);
        stepText.SetText(data.Step.ToString());
        speedText.SetText(data.Speed.ToString());
        defenseText.SetText(data.defense.ToString());
        dodgeText.SetText(data.Dodge.ToString());
        stunResistText.SetText(data.StunResist.ToString());
        bleedResistText.SetText(data.BleedResist.ToString());
        poisonResistText.SetText(data.PoisonResist.ToString());

        skillIndexText.SetText((character.techIndex + 1).ToString());
        skillNameText.SetText(character.equippedTech.ID);
        switch (character.equippedTech.TechType)
        {
            case TechType.Attack:
                skillTypeText.SetText("물리 공격");
                break;

            case TechType.Stress:
                skillTypeText.SetText("스트레스 공격");
                break;

            case TechType.Heal:
                skillTypeText.SetText("체력 힐");
                break;

            case TechType.StressHeal:
                skillTypeText.SetText("스트레스 힐");
                break;
        }
        switch (character.equippedTech.TechTarget)
        {
            case TechTarget.Single:
                skillTargetText.SetText("단일");
                break;

            case TechTarget.Multiple:
                skillTargetText.SetText("다중");
                break;
        }
        int crit;
        // 캐릭터 크리티컬 + 기술별 크리티컬 보정치
        if (character.equippedTech.isFixedCritical)
        {
            crit = character.equippedTech.FixedCritical;
        }
        else
        {
            crit = character.characterData.Crit + character.equippedTech.CriticalMod;
        }
        if (crit < 0)
            crit = 0;
        skillCriticalText.SetText(crit.ToString());
        stunText.SetText(character.equippedTech.Stun.ToString());
        bleedText.SetText(character.equippedTech.Bleed.ToString());
        bleedDescText.SetText(string.Format("{0} 턴당 {1} 데미지", character.equippedTech.BleedTurnCnt, character.equippedTech.BleedDamage));
        poisonText.SetText(character.equippedTech.Poison.ToString());
        poisonDescText.SetText(string.Format("{0} 턴당 {1} 데미지", character.equippedTech.PoisonTurnCnt, character.equippedTech.PoisonDamage));

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void hideStat()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
