using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StateUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statText;
    [SerializeField] GameObject SkillImageSet;
    [SerializeField] TextMeshProUGUI skillText;

    void Awake()
    {
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowStat(Hero hero)
    {
        // string statTxt = string.Format("걸음 수:    {0}\n기절 저항력:   {1}\n출혈 저항력:   {2}\n중독 저항력:   {3}",
        // hero.step, hero.characterData.StunResist, hero.characterData.BleedResist, hero.characterData.PoisonResist);
        // statText.SetText(statTxt);

        // string skillTxt = string.Format("타입:  {6}\n데미지 범위:   {0}~{1}\n타겟:  {2}\n기절:  {3}\n출혈:  {4}\n중독:  {5}",
        // hero.equippedTech.minDamage, hero.equippedTech.maxDamage, hero.equippedTech.TechTarget, hero.equippedTech.Stun, hero.equippedTech.Bleed, hero.equippedTech.Poison, hero.equippedTech.TechType);
        // skillText.SetText(skillTxt);

        // SkillImageSet.SetActive(true);
        // gameObject.SetActive(true);
        // transform.position = hero.transform.position + new Vector3(0, 5, 0);
    }

    public void ShowStat(EnemyAI enemy)
    {
        // string statTxt = string.Format("걸음 수:    {0}\n기절 저항력:   {1}\n출혈 저항력:   {2}\n중독 저항력:   {3}",
        // enemy.step, enemy.characterData.StunResist, enemy.characterData.BleedResist, enemy.characterData.PoisonResist);
        // statText.SetText(statTxt);

        // string skillTxt = string.Format("타입:  {6}\n데미지 범위:   {0}~{1}\n타겟:  {1}\n기절:  {2}\n출혈:  {3}\n중독:  {4}",
        // enemy.equippedTech.minDamage, enemy.equippedTech.maxDamage, enemy.equippedTech.TechTarget, enemy.equippedTech.Stun, enemy.equippedTech.Bleed, enemy.equippedTech.Poison, enemy.equippedTech.TechType);
        // skillText.SetText(skillTxt);

        // SkillImageSet.SetActive(false);
        // gameObject.SetActive(true);
        // transform.position = enemy.transform.position + new Vector3(0, -5, 0);
    }

    public void hideStat()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
