using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroUI : MonoBehaviour
{
    [SerializeField] Hero hero;

    [SerializeField] GameObject turnUI;
    [SerializeField] GameObject stunUI;
    [SerializeField] GameObject bleedUI;
    [SerializeField] GameObject poisonUI;
    [SerializeField] Slider hpGauge;
    [SerializeField] Slider stressGauge;

    GameObject hoverObject;
    private RectTransform hoverRectTransform;
    private RectTransform canvasRectTransform;

    void Awake()
    {
    }

    public void Init(Hero hero)
    {
        this.hero = hero;
        hoverObject = UIManager.Instance.MiniStatue.transform.parent.gameObject;
        hoverRectTransform = UIManager.Instance.MiniStatue.transform.parent.GetComponent<RectTransform>();
        // Canvas의 RectTransform 가져오기
        canvasRectTransform = hoverObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        hpGauge.maxValue = hero.characterData.MaxHp;
        hpGauge.value = hero.hp;
        stressGauge.maxValue = 200;
        stressGauge.value = hero.characterData.Stress;

        hpGauge.GetComponent<UIHoverHandler>().OnHoverEnter += OnHoverEnter;
        hpGauge.GetComponent<UIHoverHandler>().OnHoverExit += OnHoverExit;

        stressGauge.GetComponent<UIHoverHandler>().OnHoverEnter += OnHoverEnter;
        stressGauge.GetComponent<UIHoverHandler>().OnHoverExit += OnHoverExit;

        stunUI.GetComponent<UIHoverHandler>().OnHoverEnter += OnHoverEnter;
        stunUI.GetComponent<UIHoverHandler>().OnHoverExit += OnHoverExit;

        bleedUI.GetComponent<UIHoverHandler>().OnHoverEnter += OnHoverEnter;
        bleedUI.GetComponent<UIHoverHandler>().OnHoverExit += OnHoverExit;

        poisonUI.GetComponent<UIHoverHandler>().OnHoverEnter += OnHoverEnter;
        poisonUI.GetComponent<UIHoverHandler>().OnHoverExit += OnHoverExit;

        stunUI.SetActive(false);
        bleedUI.SetActive(false);
        poisonUI.SetActive(false);
    }

    void OnHoverEnter(GameObject ui, Vector3 mousePosition)
    {
        string str = "";

        if (ui == hpGauge.gameObject)
        {
            str = string.Format("현재 체력: {0}", hpGauge.value);
        }
        else if (ui == stressGauge.gameObject)
        {
            str = string.Format("현재 스트레스: {0}", stressGauge.value);
        }
        else if (ui == stunUI)
        {
            str = string.Format("기절 상태");
        }
        else if (ui == bleedUI)
        {
            Tuple<int, int> result = hero.GetBleedState();
            str = string.Format("출혈 상태 | 데미지: {0} | 남은 턴 수: {1}", result.Item1, result.Item2);
        }
        else if (ui == poisonUI)
        {
            Tuple<int, int> result = hero.GetPoisonState();
            str = string.Format("중독 상태 | 데미지: {0} | 남은 턴 수: {1}", result.Item1, result.Item2);
        }

        if (UIManager.Instance.MiniStatue.transform.parent.gameObject.activeSelf)
            UIManager.Instance.MiniStatue.SetText(str);
        else
        {
            UIManager.Instance.MiniStatue.transform.parent.gameObject.SetActive(true);
            UIManager.Instance.MiniStatue.SetText(str);
        }

        PositionHoverObject(mousePosition);
    }

    void OnHoverExit(GameObject ui, Vector3 position)
    {
        if (UIManager.Instance.MiniStatue.transform.parent.gameObject.activeSelf)
            UIManager.Instance.MiniStatue.transform.parent.gameObject.SetActive(false);

    }

    private void PositionHoverObject(Vector3 mousePosition)
    {
        Vector3 newPosition = mousePosition + new Vector3(0, 1, 0); // 마우스 오른쪽으로 1만큼 이동

        // 화면 경계 체크
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)hoverObject.transform.parent, newPosition, null, out Vector2 localPoint);
        hoverRectTransform.localPosition = localPoint;

        float canvasWidth = ((RectTransform)hoverObject.transform.parent).rect.width;
        float hoverWidth = hoverRectTransform.rect.width;

        // 화면 오른쪽 경계 체크
        if (hoverRectTransform.localPosition.x + hoverWidth > canvasWidth)
        {
            hoverRectTransform.localPosition = new Vector3(canvasWidth - hoverWidth, hoverRectTransform.localPosition.y, hoverRectTransform.localPosition.z);
        }
    }


}