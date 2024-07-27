using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterDescUI : MonoBehaviour
{
    TextMeshProUGUI textMeshPro;
    float displayDuration = 1.5f;
    float fadeDuration = 0.5f;

    void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();

        StartCoroutine(FadeText());
    }

    private IEnumerator FadeText()
    {
        // 텍스트를 즉시 완전히 불투명하게 설정
        textMeshPro.alpha = 1;

        // displayDuration 동안 텍스트 유지
        yield return new WaitForSeconds(displayDuration);

        // fadeDuration 동안 점차 투명하게 설정
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            textMeshPro.alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            yield return null;
        }

        // 완전히 투명하게 설정
        textMeshPro.alpha = 0;

        Destroy(gameObject);
    }


}
