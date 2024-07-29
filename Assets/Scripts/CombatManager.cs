using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : SingleTon<CombatManager>
{
    // Coroutine CameraCorouine;
    Coroutine ZoomIn;
    Coroutine ZoomOut;
    int maxPercent = 101;

    // 단일 공격
    public void Combat(Character attacker, Character taker)
    {
        if (ZoomIn == null)
            ZoomIn = StartCoroutine(ZoomInCamera(attacker.gameObject));
        else
        {
            StopCoroutine(ZoomIn);
            ZoomIn = StartCoroutine(ZoomInCamera(attacker.gameObject));
        }

        // 명중
        if (isHit(attacker.characterData, taker.characterData, attacker.equippedTech))
        {
            // 크리티컬 여부
            bool crit = isCritical(attacker);

            if (crit)
            {
                attacker.OnDidCritical();
            }

            switch (attacker.equippedTech.TechType)
            {
                case TechType.Attack:
                    int damage = GetDamage(attacker, taker.characterData, crit);
                    taker.OnDamaged(damage, crit);
                    break;

                case TechType.Stress:
                    int stress = GetDamage(attacker, taker.characterData, crit);
                    taker.OnStressed(stress, crit);
                    break;

                case TechType.Heal:
                    int heal = GetHeal(attacker, crit);
                    taker.OnHealed(heal, crit);
                    break;

                case TechType.StressHeal:
                    int stressheal = GetHeal(attacker, crit);
                    taker.OnStressHealed(stressheal, crit);
                    break;
            }

            // 기술 별 상태 이상 확인 -> 크리티컬이면 발동 확률 높이기
            // 기절
            if (isStun(attacker.equippedTech, taker.characterData, crit))
            {
                taker.OnStun(true, true);
            }
            else
            {
                // 스턴 효과가 있는데 발동 안됨
                if (attacker.equippedTech.isStunEnable)
                    taker.OnStun(false, true);
            }

            // 출혈
            // 크리티컬이면 50% 지속 시간 증가
            if (isBleed(attacker.equippedTech, taker.characterData, crit))
            {
                taker.OnBleed(true, true, attacker.equippedTech.BleedDamage, attacker.equippedTech.BleedTurnCnt + (crit ? attacker.equippedTech.BleedTurnCnt / 2 : 0));
            }
            else
            {
                // 출혈 효과가 있는데 발동 안됨
                if (attacker.equippedTech.isBleedEnable)
                    taker.OnBleed(false, true);
            }

            // 중독
            // 크리티컬이면 50% 지속 시간 증가
            if (isPoision(attacker.equippedTech, taker.characterData, crit))
            {
                taker.OnPoison(true, true, attacker.equippedTech.PoisonDamage, attacker.equippedTech.PoisonTurnCnt + (crit ? attacker.equippedTech.PoisonTurnCnt / 2 : 0));
            }
            else
            {
                // 중독 효과가 있는데 발동 안됨
                if (attacker.equippedTech.isPoisonEnable)
                    taker.OnPoison(false, true);
            }
        }
        // 회피
        else
        {
            if (ZoomIn == null)
                ZoomIn = StartCoroutine(ZoomInCamera(attacker.gameObject));
            else
            {
                StopCoroutine(ZoomIn);
                ZoomIn = StartCoroutine(ZoomInCamera(attacker.gameObject));
            }
            taker.OnDodged(attacker.equippedTech.TechType);
        }
    }

    // 다중 공격
    public void Combat(Character attaker, List<Character> takers)
    {
        foreach (Character c in takers)
        {
            Combat(attaker, c);
        }
    }


    bool isHit(CharacterData attacker, CharacterData taker, TechData attack)
    {
        if (attack.TechType != TechType.Heal && attack.TechType != TechType.StressHeal)
        {
            // 명중률 = 기술 명중 + 캐릭터 명중 보정치 - 적 회피
            float accuracy = attack.Acc + attacker.AccMod - taker.Dodge;

            if (Random.Range(0.0f, 1f) <= accuracy)
            {
                // 명중
                return true;
            }
            else
            {
                // 회피
                return false;
            }
        }
        else
            return true;

    }

    bool isCritical(Character attacker)
    {
        CharacterData attackerData = attacker.characterData;
        TechData attack = attacker.equippedTech;

        // 캐릭터 크리티컬 + 기술별 크리티컬 보정치
        if (attack.isFixedCritical)
        {
            return Random.Range(0, maxPercent) <= attack.FixedCritical;
        }
        else
        {
            return Random.Range(0, maxPercent) <= attackerData.Crit + attack.CriticalMod;
        }

    }

    bool isStun(TechData techData, CharacterData taker, bool isCritical)
    {
        // 애초에 스턴 기능이 있어야 함
        if (techData.isStunEnable)
        {
            // 기절 수치 = 기술 기절 + 크리티컬 성공 시 20 추가 - 적 기절 저항력
            int stunPercent = techData.Stun + (isCritical ? 20 : 0) - taker.StunResist;

            return Random.Range(0, maxPercent) <= stunPercent;
        }
        else return false;
    }
    bool isBleed(TechData techData, CharacterData taker, bool isCritical)
    {
        // 애초에 출혈 기능이 있어야 함
        if (techData.Bleed > 0)
        {
            // 출혈 수치 = 출혈 기절 - 적 출혈 저항력
            int bleedPercent = techData.Bleed + (isCritical ? 20 : 0) - taker.BleedResist;
            return Random.Range(0, maxPercent) <= bleedPercent;
        }
        else return false;
    }
    bool isPoision(TechData techData, CharacterData taker, bool isCritical)
    {
        // 애초에 중독 기능이 있어야 함
        if (techData.Poison > 0)
        {
            // 기절 수치 = 기술 기절 - 적 기절 저항력
            int poisonPercent = techData.Poison + (isCritical ? 20 : 0) - taker.PoisonResist;

            return Random.Range(0, maxPercent) <= poisonPercent;
        }
        else
        {
            return false;
        }
    }

    int GetDamage(Character attacker, CharacterData taker, bool isCritical)
    {
        CharacterData attackerData = attacker.characterData;
        TechData attack = attacker.equippedTech;
        int defaultDmg;

        // 공격 데미지 = 캐릭터 데미지 * 기술별 데미지 보정치
        // 최종 데미지 = 공격 데미지 - ((100 - 방어력)/100)

        // 크리티컬: 최대 데미지 * 1.5 소수점 반올림
        if (isCritical)
        {
            if (!attack.isFixedDamage)
                defaultDmg = Mathf.RoundToInt(attackerData.maxDamage * (attack.dMGMod == Mod.positive ? (1 + attack.DamageMod) : (1 - attack.DamageMod)) * 1.5f);
            else
                defaultDmg = Mathf.RoundToInt(attack.FixedMaxDamage * (attack.dMGMod == Mod.positive ? (1 + attack.DamageMod) : (1 - attack.DamageMod)) * 1.5f);

            if (attack.TechType == TechType.Attack)
                return Mathf.RoundToInt(defaultDmg * ((100 - taker.defense) / 100));
            else
                return defaultDmg;
        }
        else
        {
            // max는 포함 아니기 때문에 1 추가

            if (!attack.isFixedDamage)
                defaultDmg
                = Mathf.RoundToInt(Random.Range(attackerData.minDamage, attackerData.maxDamage + 1) * (attack.dMGMod == Mod.positive ? (1 + attack.DamageMod) : (1 - attack.DamageMod)));
            else
                defaultDmg
                = Mathf.RoundToInt(Random.Range(attack.FixedMinDamage, attack.FixedMaxDamage + 1) * (attack.dMGMod == Mod.positive ? (1 + attack.DamageMod) : (1 - attack.DamageMod)));

            if (attack.TechType == TechType.Attack)
                return Mathf.RoundToInt(defaultDmg * ((100 - taker.defense) / 100));
            else
                return defaultDmg;
        }
    }

    int GetHeal(Character attacker, bool isCritical)
    {
        CharacterData attackerData = attacker.characterData;
        TechData attack = attacker.equippedTech;
        int defaultHeal;
        // 힐은 결과값에 1.5배 반올림
        if (!attack.isFixedDamage)
        {
            defaultHeal = Random.Range(attackerData.minDamage, attackerData.maxDamage + 1);
        }
        else
        {
            defaultHeal = Random.Range(attack.FixedMinDamage, attack.FixedMaxDamage + 1);
        }

        if (isCritical)
        {
            return Mathf.RoundToInt(defaultHeal * 1.5f);
        }
        else
        {
            return defaultHeal;
        }
    }

    IEnumerator ZoomInCamera(GameObject attacker)
    {
        if (UIManager.Instance.CharacterUIs.activeSelf)
        {
            UIManager.Instance.MiniStatue.transform.parent.gameObject.SetActive(false);
            UIManager.Instance.CharacterUIs.SetActive(false);
        }

        Vector3 initialCameraPosition = Camera.main.transform.position;
        Vector3 targetCameraPosition = new Vector3(attacker.transform.position.x, attacker.transform.position.y, Camera.main.transform.position.z);
        float initialOrthographicSize = Camera.main.orthographicSize;
        float targetOrthographicSize = 4f;
        float zoomInDuration = 0.2f;
        float elapsedTime = 0f;
        while (elapsedTime < zoomInDuration)
        {
            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;

            // 비율 계산 (0.0에서 1.0 사이)
            float t = Mathf.Clamp01(elapsedTime / zoomInDuration);

            Camera.main.transform.position = Vector3.Lerp(initialCameraPosition, targetCameraPosition, t);
            Camera.main.orthographicSize = Mathf.Lerp(initialOrthographicSize, targetOrthographicSize, t);
            yield return null;
        }

        Camera.main.transform.position = targetCameraPosition;
        Camera.main.orthographicSize = targetOrthographicSize;
    }

    IEnumerator ZoomOutCamera(Character attacker)
    {
        Vector3 initialCameraPosition = Camera.main.transform.position;
        float initialOrthographicSize = Camera.main.orthographicSize;
        float targetOrthographicSize = 8f;
        float zoomInDuration = 0.2f;
        float elapsedTime = 0f;
        while (elapsedTime < zoomInDuration)
        {
            // 경과 시간 업데이트
            elapsedTime += Time.deltaTime;

            // 비율 계산 (0.0에서 1.0 사이)
            float t = Mathf.Clamp01(elapsedTime / zoomInDuration);

            Camera.main.transform.position = Vector3.Lerp(initialCameraPosition, new Vector3(0, 0, -10), t);
            Camera.main.orthographicSize = Mathf.Lerp(initialOrthographicSize, targetOrthographicSize, t);
            yield return null;
        }

        Camera.main.transform.position = new Vector3(0, 0, -10);
        Camera.main.orthographicSize = targetOrthographicSize;

        if (!UIManager.Instance.CharacterUIs.activeSelf)
        {
            UIManager.Instance.CharacterUIs.SetActive(true);
        }

        attacker.GoToNextTurn();
    }

    public void CallZoomOutCamera(Character c)
    {
        if (ZoomOut == null)
        {
            ZoomOut = StartCoroutine(ZoomOutCamera(c));
        }
        else
        {
            StopCoroutine(ZoomOut);
            ZoomOut = StartCoroutine(ZoomOutCamera(c));
        }
    }
}
