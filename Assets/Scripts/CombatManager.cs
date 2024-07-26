using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatManager : SingleTon<CombatManager>
{
    [SerializeField] GameObject window;
    [SerializeField] GameObject shortDamageEffect;
    TextMeshProUGUI combatInfo;

    void Awake()
    {
        combatInfo = GameObject.Find("CombatInfo").GetComponent<TextMeshProUGUI>();
        combatInfo.gameObject.SetActive(false);
    }

    public void ShowWindow()
    {
        Camera.main.transform.position = new Vector3(window.transform.position.x, window.transform.position.y, Camera.main.transform.position.z);
    }

    public void Combat(Character attacker, Character taker)
    {
        if (isHit(attacker.characterData, taker.characterData, attacker.equippedTech))
        {
            StartCoroutine(ZoomInCamera(taker.gameObject, shortHit(taker.gameObject), "히트"));
            // StartCoroutine(showText(taker.gameObject, "Hit"));
            // StartCoroutine(shortHit(taker.gameObject));
        }
        else
        {
            StartCoroutine(ZoomInCamera(taker.gameObject, dodge(taker.gameObject), "회피!"));
            // StartCoroutine(showText(taker.gameObject, "Dodge!"));
            // StartCoroutine(dodge(taker.gameObject));
        }
    }

    public void Combat(Character attaker, List<Character> taker)
    {

    }

    // 다중 히트
    void CalculateAttack(CharacterData attacker, List<CharacterData> takers, TechData attack)
    {
    }

    // 히트
    bool isHit(CharacterData attacker, CharacterData taker, TechData attack)
    {
        // ShowWindow();

        // 명중률 = 기술 명중 + 캐릭터 명중 보정치 - 적 회피
        float accuracy = attack.Acc + attacker.AccMod - taker.Dodge;

        if (Random.Range(0.0f, 1f) < accuracy)
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

    IEnumerator ZoomInCamera(GameObject taker, IEnumerator callback, string text)
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
        StartCoroutine(showText(taker, text));
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

    IEnumerator showText(GameObject taker, string text)
    {
        combatInfo.SetText(text);
        combatInfo.transform.position = taker.transform.position + new Vector3(0, taker.transform.localScale.y);
        combatInfo.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        combatInfo.gameObject.SetActive(false);

    }

    IEnumerator shortHit(GameObject taker)
    {
        GameObject go = Instantiate(shortDamageEffect, taker.transform.position, shortDamageEffect.transform.rotation);

        yield return new WaitForSeconds(1f);

        Destroy(go);

        StartCoroutine(ZoomOutCamera());
    }

    IEnumerator dodge(GameObject taker)
    {
        Vector3 originPose = taker.transform.position;
        Vector3 endPose = taker.transform.position + new Vector3(1.5f, 0, 0);

        GameObject go = Instantiate(shortDamageEffect, originPose, shortDamageEffect.transform.rotation);

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
        Destroy(go);

        StartCoroutine(ZoomOutCamera());
    }
}
