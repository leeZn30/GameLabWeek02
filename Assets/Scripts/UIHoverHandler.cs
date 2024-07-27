using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public delegate void HoverAction(GameObject uiElement, Vector3 position);
    public event HoverAction OnHoverEnter;
    public event HoverAction OnHoverExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 마우스 오버 시 실행할 코드
        OnHoverEnter?.Invoke(gameObject, mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 떠날 때 실행할 코드
        OnHoverExit?.Invoke(gameObject, Vector3.zero);
    }
}
