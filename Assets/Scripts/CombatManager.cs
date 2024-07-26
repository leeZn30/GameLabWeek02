using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CombatManager : SingleTon<CombatManager>
{
    [SerializeField] GameObject window;
    [SerializeField] GameObject meleeAtk;
    [SerializeField] GameObject rangedAtk;
    [SerializeField] GameObject meleeStress;
    [SerializeField] GameObject rangedStress;
    [SerializeField] GameObject heal;
    int maxPercent = 101;

    // 단일 공격
    public void Combat(Character attacker, Character taker)
    {
        // 공격 방식 + 범위에 따른 연출
        IEnumerator callback = null;

        // 연출 추가
        switch (attacker.equippedTech.TechType)
        {
            case TechType.Attack:
                if (attacker.equippedTech.TechRange == TechRange.Melee)
                {
                    callback = meleeAtkHit(taker.gameObject);
                }
                else
                {
                    // callback = rangedAtkHit(attacker.gameObject, taker.gameObject);
                    callback = meleeAtkHit(taker.gameObject);
                }
                break;

            case TechType.Stress:
                if (attacker.equippedTech.TechRange == TechRange.Melee)
                {
                    callback = meleeStressHit(taker.gameObject);
                }
                else
                {
                    // callback = rangedStressHit(attacker.gameObject, taker.gameObject);
                    callback = meleeStressHit(taker.gameObject);
                }
                break;

            case TechType.Heal:
                callback = healHit(taker.gameObject);
                break;
        }

        // 명중
        if (isHit(attacker.characterData, taker.characterData, attacker.equippedTech))
        {
            // 크리티컬 여부
            bool crit = isCritical(attacker);

            /* 크리티컬이면
            - attacker는 스트레스 3 회복
            - attacker 주변 아군도 25% 확률로 3 회복
            */
            if (crit)
            {
                attacker.OnStressed(-3);

                UIManager.Instance.AddCombatInfo("<color=black>-3");
                // 주변 2칸 이내 아군 회복
            }

            switch (attacker.equippedTech.TechType)
            {
                case TechType.Attack:
                    int damage = GetDamage(attacker.equippedTech, crit);
                    taker.OnDamaged(damage, crit);
                    if (crit)
                        UIManager.Instance.AddCombatInfo(string.Format("<color=red>치명타!\n-{0}", damage), 0);
                    else
                        UIManager.Instance.AddCombatInfo(string.Format("<color=red>-{0}", damage), 0);
                    break;

                case TechType.Stress:
                    int stress = GetDamage(attacker.equippedTech, crit);
                    taker.OnStressed(stress);

                    if (crit)
                        UIManager.Instance.AddCombatInfo(string.Format("<color=black>치명타!\n+{0}", stress), 0);
                    else
                        UIManager.Instance.AddCombatInfo(string.Format("<color=black>+{0}", stress), 0);
                    break;

                case TechType.Heal:
                    int heal = GetHeal(attacker.equippedTech, crit);
                    taker.OnHealed(heal);

                    if (crit)
                        UIManager.Instance.AddCombatInfo(string.Format("<color=#9BFF00>치명타!\n+{0}", heal), 0);
                    else
                        UIManager.Instance.AddCombatInfo(string.Format("<color=#9BFF00>+{0}", heal), 0);
                    break;
            }

            // 기술 별 상태 이상 확인 -> 크리티컬이면 발동 확률 높이기
            // 기절
            if (isStun(attacker.equippedTech, taker.characterData, crit))
            {
                taker.OnStun();
                UIManager.Instance.AddCombatInfo("<color=yellow>기절!");
            }
            else
            {
                // 스턴 효과가 있는데 발동 안됨
                if (attacker.equippedTech.Stun > 0)
                    UIManager.Instance.AddCombatInfo("<color=yellow>기절 저항");
            }

            // 출혈
            // 크리티컬이면 50% 지속 시간 증가
            if (isBleed(attacker.equippedTech, taker.characterData, crit))
            {
                taker.OnBleed(attacker.equippedTech.BleedDamage, attacker.equippedTech.BleedTurnCnt + (crit ? attacker.equippedTech.BleedTurnCnt / 2 : 0));
                UIManager.Instance.AddCombatInfo("<color=#C100A5>출혈");
            }
            else
            {
                // 출혈 효과가 있는데 발동 안됨
                if (attacker.equippedTech.Bleed > 0)
                    UIManager.Instance.AddCombatInfo("<color=#C100A5>출혈 저항");
            }

            // 중독
            // 크리티컬이면 50% 지속 시간 증가
            if (isPoision(attacker.equippedTech, taker.characterData, crit))
            {
                taker.OnPoison(attacker.equippedTech.PoisonDamage, attacker.equippedTech.PoisonTurnCnt + (crit ? attacker.equippedTech.PoisonTurnCnt / 2 : 0));
                UIManager.Instance.AddCombatInfo("<color=green>중독");
            }
            else
            {
                // 중독 효과가 있는데 발동 안됨
                if (attacker.equippedTech.Poison > 0)
                    UIManager.Instance.AddCombatInfo("<color=green>중독 저항");
            }

            // 연출 시작
            StartCoroutine(ZoomInCamera(taker.gameObject, callback));
        }
        // 회피
        else
        {
            UIManager.Instance.AddCombatInfo("회피!");
            StartCoroutine(ZoomInCamera(taker.gameObject, callback, dodge(taker.gameObject)));
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
        if (attack.TechType != TechType.Heal)
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
        if (Random.Range(0, 101) <= attacker.crit)
            return true;
        else return false;
    }

    bool isStun(TechData techData, CharacterData taker, bool isCritical)
    {
        // 애초에 스턴 기능이 있어야 함
        if (techData.Stun > 0)
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

    int GetDamage(TechData attack, bool isCritical)
    {
        // max는 포함 아니기 때문에 1 추가
        // 일반 공격은 최대 데미지 * 1.5 소수점 반올림
        if (isCritical)
        {
            return Mathf.RoundToInt(attack.maxDamage * 1.5f);
        }
        else
        {

            return Random.Range(attack.minDamage, attack.maxDamage + 1);
        }
    }

    int GetHeal(TechData attack, bool isCritical)
    {
        // 힐은 결과값에 1.5배 반올림
        int heal = Random.Range(attack.minDamage, attack.maxDamage + 1);

        if (isCritical)
        {
            return Mathf.RoundToInt(heal * 1.5f);
        }
        else
        {

            return heal;
        }
    }

    IEnumerator ZoomInCamera(GameObject taker, IEnumerator callback, IEnumerator dodge = null)
    {
        Vector3 initialCameraPosition = Camera.main.transform.position;
        Vector3 targetCameraPosition = new Vector3(taker.transform.position.x, taker.transform.position.y, Camera.main.transform.position.z);
        float initialOrthographicSize = Camera.main.orthographicSize;
        float targetOrthographicSize = 4f;
        float zoomInDuration = 0.5f;
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

        StartCoroutine(callback);
        if (dodge != null)
            StartCoroutine(dodge);

        UIManager.Instance.ShowCombatInfos(taker.transform.position + new Vector3(0, taker.transform.localScale.y));
    }

    IEnumerator ZoomOutCamera()
    {
        Vector3 initialCameraPosition = Camera.main.transform.position;
        float initialOrthographicSize = Camera.main.orthographicSize;
        float targetOrthographicSize = 8f;
        float zoomInDuration = 0.5f;
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
    }

    public void CallZoomOutCamera()
    {
        StartCoroutine(ZoomOutCamera());
    }

    IEnumerator meleeAtkHit(GameObject taker)
    {
        GameObject go = Instantiate(meleeAtk, taker.transform.position, meleeAtk.transform.rotation);

        yield return new WaitForSeconds(1f);

        Destroy(go);
    }

    IEnumerator rangedAtkHit(GameObject attacker, GameObject taker)
    {
        GameObject go = Instantiate(rangedAtk, attacker.transform.position, rangedAtk.transform.rotation);

        // 회전 및 이동을 시작
        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 이동 보간 (Lerp)
            go.transform.position = Vector3.Lerp(attacker.transform.position, taker.transform.position, elapsedTime / duration);

            yield return null; // 다음 프레임까지 대기
        }

        yield return new WaitForSeconds(0.8f);

        // 최종 위치 및 회전 설정
        Destroy(go);
    }

    IEnumerator meleeStressHit(GameObject taker)
    {
        GameObject go = Instantiate(meleeStress, taker.transform.position, meleeStress.transform.rotation);

        yield return new WaitForSeconds(1f);

        Destroy(go);
    }

    IEnumerator rangedStressHit(GameObject attacker, GameObject taker)
    {

        GameObject go = Instantiate(rangedStress, attacker.transform.position, rangedStress.transform.rotation);

        // 회전 및 이동을 시작
        float elapsedTime = 0f;
        float duration = 0.2f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // 이동 보간 (Lerp)
            go.transform.position = Vector3.Lerp(attacker.transform.position, taker.transform.position, elapsedTime / duration);

            yield return null; // 다음 프레임까지 대기
        }

        yield return new WaitForSeconds(0.8f);

        // 최종 위치 및 회전 설정
        Destroy(go);

    }

    IEnumerator healHit(GameObject taker)
    {
        GameObject go = Instantiate(heal, taker.transform.position, heal.transform.rotation);

        yield return new WaitForSeconds(1f);

        Destroy(go);
    }

    IEnumerator dodge(GameObject taker)
    {
        Vector3 originPose = taker.transform.position;
        Vector3 endPose = taker.transform.position + new Vector3(1.5f, 0, 0);


        float duration = 0.3f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);

            // 위치 보간 (Lerp를 사용하여 대각선으로 자연스럽게 이동)
            taker.transform.position = Vector3.Lerp(originPose, endPose, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.7f);

        taker.transform.position = originPose;
    }
}
