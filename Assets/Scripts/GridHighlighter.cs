using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridHighlighter : MonoBehaviour
{
    public Hero selectedHero;
    public Vector3Int PrevHighlightedPosition;

    Tilemap tilemap;
    LineRenderer lineRenderer;

    Stack<Vector3Int> highlightedTilePositions = new Stack<Vector3Int>();

    [Header("사용 오브젝트")]
    [SerializeField] Tile highlightedTile;
    [SerializeField] Tile originalTile;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (selectedHero != null)
        {
            HighlightMovableTiles();
        }
    }

    void HighlightMovableTiles()
    {
        // 마우스 위치에서 타일을 가져옴
        Vector3Int mousePosition = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        // 표시된 타일이 있으며, 그 전 표시된 타일로 마우스를 옮긴다면 가장 마지막 타일의 표시를 지운다.
        if (highlightedTilePositions.Count > 0 && mousePosition == PrevHighlightedPosition)
        {
            tilemap.SetTile(highlightedTilePositions.Pop(), originalTile);

            // 이전 위치 갱신
            if (highlightedTilePositions.Count > 0)
                PrevHighlightedPosition = highlightedTilePositions.Peek();
            else
                PrevHighlightedPosition = selectedHero.CurrentTilePosition;

        }

        if (highlightedTilePositions.Count < selectedHero.StepCount)
        {
            // 
            if (mousePosition != selectedHero.CurrentTilePosition && !highlightedTilePositions.Contains(mousePosition))
            {
                // 이전 위치 갱신
                if (highlightedTilePositions.Count > 0)
                    PrevHighlightedPosition = highlightedTilePositions.Peek();
                else
                    PrevHighlightedPosition = selectedHero.CurrentTilePosition;

                highlightedTilePositions.Push(mousePosition);

                // 해당 타일 타일베이스 변경
                tilemap.SetTile(mousePosition, highlightedTile);

                Debug.Log(highlightedTilePositions.Count);
            }
        }

        lineRenderer.positionCount = highlightedTilePositions.Count;
        lineRenderer.SetPositions(ConvertStackToList(highlightedTilePositions).ToArray());
    }

    public void HighlightStartTile(Vector3Int position)
    {
        highlightedTilePositions.Clear();
        highlightedTilePositions.Push(position);
        PrevHighlightedPosition = position;
    }

    public void HighlightSpecificTile(Vector3Int position)
    {
        if (tilemap.GetTile(position) != highlightedTile)
        {
            tilemap.SetTile(position, highlightedTile);
        }
    }
    public void UnHighlightSpecificTile(Vector3Int position)
    {
        if (tilemap.GetTile(position) != originalTile)
        {
            tilemap.SetTile(position, originalTile);
        }
    }

    bool CanGo(Vector3Int position)
    {
        /*
        * 1. 4방향(상, 하, 좌, 우)에 있어야 함
        * 2. 적이 없어야 함
        * 3. 한번 갔던 곳이면 안됨
        */
        return true;
    }

    // Stack은 참조 형식으로 전달됨 -> 원본에 영향
    List<Vector3> ConvertStackToList(Stack<Vector3Int> stack)
    {
        List<Vector3> list = new List<Vector3>();
        Stack<Vector3Int> tmp = new Stack<Vector3Int>(stack);
        while (tmp.Count > 0)
        {
            list.Add((Vector3)tmp.Pop() + new Vector3(0.5f, 0.5f, 0));
        }
        return list;
    }

}
