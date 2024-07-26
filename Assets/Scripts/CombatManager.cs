using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : SingleTon<CombatManager>
{
    [SerializeField] GameObject window;
    [SerializeField] GameObject shortDamageEffect;
    GameObject combatInfo;

    void Awake()
    {
        combatInfo = GameObject.Find("CombatInfo");
        combatInfo.SetActive(false);
    }

    public void ShowWindow()
    {
        Camera.main.transform.position = new Vector3(window.transform.position.x, window.transform.position.y, Camera.main.transform.position.z);
    }

    public void Combat(GameObject attaker, GameObject taker)
    {

    }

    public void Combat(GameObject attaker, List<GameObject> taker)
    {

    }

    // 다중 히트
    void CalculateAttack(CharacterData attacker, List<CharacterData> takers, TechData attack)
    {
    }

    // 히트
    void CalculateAttack(CharacterData attacker, CharacterData taker, TechData attack)
    {
        // ShowWindow();

        // 명중률 = 기술 명중 + 캐릭터 명중 보정치 - 적 회피
        float accuracy = attack.Acc + attacker.AccMod - taker.Dodge;

        if (Random.Range(0.0f, 1f) < accuracy)
        {
            // 명중
            Debug.Log("명중");


            // 기술에 따른 보조 효과 작동
        }
        else
        {
            // 회피
            Debug.Log("회피!");
        }
    }

    public void ShowProduction(GameObject taker)
    {
        StartCoroutine(shortHit(taker));
    }

    IEnumerator shortHit(GameObject taker)
    {
        GameObject go = Instantiate(shortDamageEffect, taker.transform.position, shortDamageEffect.transform.rotation);
        combatInfo.transform.position = taker.transform.position + new Vector3(0, taker.transform.localScale.y);
        combatInfo.SetActive(true);

        yield return new WaitForSeconds(1f);

        Destroy(go);
        combatInfo.SetActive(false);
    }
}
