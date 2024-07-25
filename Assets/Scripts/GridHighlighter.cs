using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridHighlighter : MonoBehaviour
{
    public Hero selectedHero;
    public Vector3Int PrevHighlightedPosition;
    public Vector3Int NowHighlightedPosition;

    Tilemap tilemap;
    LineRenderer lineRenderer;

    [Header("표시된 타일 정보")]
    HashSet<Vector3Int> enemyTiles = new HashSet<Vector3Int>();

    [Header("표시된 타일 정보")]
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

            // hero의 원래 위치가 아니고, 갈 수 있는 곳이며, 마우스가 클릭되면 해당 위치로 이동
            if (Input.GetMouseButton(0))
            {
                Vector3Int targetPosition = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (highlightedTilePositions.Contains(targetPosition) && targetPosition != selectedHero.CurrentTilePosition)
                {
                    selectedHero.transform.position = targetPosition + new Vector3(0.5f, 0.5f, 0);

                    selectedHero.UnselectHero();
                }
            }
        }
    }

    void HighlightMovableTiles()
    {
        // 마우스 위치에서 타일을 가져옴
        Vector3Int mousePosition = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        // 이전 위치로 가지 않음
        if (mousePosition != PrevHighlightedPosition)
        {
            // 현재 위치와도 다른 곳이며, stepCount보다 낮을때(기본으로 원래 위치가 들어가서 +1)
            if (mousePosition != NowHighlightedPosition && highlightedTilePositions.Count < selectedHero.StepCount + 1)
            {
                if (CanGo(mousePosition))
                {
                    // 이전 위치 갱신
                    PrevHighlightedPosition = NowHighlightedPosition;

                    // 해당 포지션 표시된 타일셋에 추가
                    highlightedTilePositions.Push(mousePosition);

                    // 해당 타일 타일베이스 변경
                    tilemap.SetTile(mousePosition, highlightedTile);

                    // 현재 위치 갱신
                    NowHighlightedPosition = mousePosition;
                }
            }
        }
        else // 이전 위치로 갔으면 빼주기
        {
            if (highlightedTilePositions.Count > 1)
                // 표시된 타일셋에서 제거
                tilemap.SetTile(highlightedTilePositions.Pop(), originalTile);

            // 현재 위치 갱신
            NowHighlightedPosition = PrevHighlightedPosition;

            // 이전 위치 갱신
            if (highlightedTilePositions.Count > 1)
                PrevHighlightedPosition = highlightedTilePositions.ElementAt(1);
            else
                PrevHighlightedPosition = highlightedTilePositions.Peek();
        }

        lineRenderer.positionCount = highlightedTilePositions.Count;
        lineRenderer.SetPositions(ConvertStackToList(highlightedTilePositions).ToArray());
    }

    bool CanGo(Vector3Int position)
    {
        /*
        * 1. 제일 마지막 방문 위치의 4방향(상, 하, 좌, 우)에 있어야 함
        * 2. 적이 없어야 함
        * 3. 한번 갔던 곳이면 안됨
        */

        // 1번 조건
        bool isLinked = false;
        Vector2Int[] dir = new Vector2Int[]
        {
            new Vector2Int(0, 1), // 상
            new Vector2Int(0, -1), // 하
            new Vector2Int(-1, 0), // 좌
            new Vector2Int(1, 0)  // 우
        };

        // 각 위치의 타일을 확인
        foreach (Vector3Int d in dir)
        {
            if (position == NowHighlightedPosition + d)
            {
                isLinked = true;
                break;
            }
        }

        // 2번 조건
        bool isNoEnemy = !enemyTiles.Contains(position);

        // 3번 조건
        bool isNoCameTile = !highlightedTilePositions.Contains(position);

        return isLinked && isNoEnemy && isNoCameTile;
    }

    public void HighlightStartTile(Vector3Int position)
    {
        // 표시 초기화
        highlightedTilePositions.Clear();

        tilemap.SetTile(position, highlightedTile);
        highlightedTilePositions.Push(position);
        PrevHighlightedPosition = position;
        NowHighlightedPosition = position;
    }

    // 표시된 모든 타일 꺼주기
    public void UnHighlightAllTile()
    {
        while (highlightedTilePositions.Count > 0)
        {
            Vector3Int position = highlightedTilePositions.Pop();

            if (tilemap.GetTile(position) != originalTile)
            {
                tilemap.SetTile(position, originalTile);
            }
        }

        lineRenderer.positionCount = 0;
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
