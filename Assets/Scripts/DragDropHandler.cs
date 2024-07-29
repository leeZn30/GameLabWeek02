using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string ID;

    private Canvas canvas;
    private RectTransform rectTransform;
    Transform originalParent;
    Hero hero;

    bool ismouseover;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();

        originalParent = transform.parent;

        GetComponent<UIHoverHandler>().OnHoverEnter += OnHoverEnter;
        GetComponent<UIHoverHandler>().OnHoverExit += OnHoverExit;
    }

    void Update()
    {
        if (ismouseover)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                hero.techIndex = 0;
                UIManager.Instance.ShowStateUI(hero);

            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                hero.techIndex = 1;
                UIManager.Instance.ShowStateUI(hero);
            }
        }
    }

    void OnHoverEnter(GameObject ui, Vector3 mousePosition)
    {
        ismouseover = true;

        hero = UnitData.Instance.characters.Find(e => e.characterData.ID == ID);
        UIManager.Instance.ShowStateUI(hero);
    }
    void OnHoverExit(GameObject ui, Vector3 mousePosition)
    {
        hero = null;
        ismouseover = false;
        UIManager.Instance.HideStateUI(UnitData.Instance.characters.Find(e => e.characterData.ID == ID));
    }

    public void OnBeginDrag(PointerEventData eventData)
    { // 드래그 시작 시 원래 부모 저장
        transform.SetParent(canvas.transform); // 드래그하는 동안 캔버스의 자식으로 설정
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 타일맵에 떨어진 위치를 계산
        Vector3 dropPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = GridHighlighter.Instance.tilemap.WorldToCell(dropPosition);

        // 드랍 위치가 타일맵의 유효한 타일인지 확인 후 캐릭터 배치
        if (GridHighlighter.Instance.tilemap.HasTile(cellPosition) && GridHighlighter.Instance.InitialPlayerPositions.Contains(cellPosition))
        {
            GridHighlighter.Instance.PlaceCharacterOnTile(cellPosition, gameObject);
            // Destroy(gameObject);
        }
        else
        {
            // 유효하지 않은 경우 원래 위치로 복귀
            rectTransform.anchoredPosition = Vector2.zero; // 원래 위치로 복귀
            transform.SetParent(originalParent); // 원래 부모로 복귀
        }
    }
}
