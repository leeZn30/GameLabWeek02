using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridHighlighter : SingleTon<GridHighlighter>
{
    public Hero selectedHero;
    public EnemyAI selectedEnemy;
    public Vector3Int PrevHighlightedPosition;
    public Vector3Int NowHighlightedPosition;

    public Tilemap tilemap;
    LineRenderer lineRenderer;

    [Header("표시된 타일 정보")]
    Stack<Vector3Int> highlightedTilePositions = new Stack<Vector3Int>();

    [Header("사용 오브젝트")]
    [SerializeField] Tile highlightedTile;
    [SerializeField] Tile originalTile;
    [SerializeField] Tile rangeTile;
    [SerializeField] GameObject attackRange;

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
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int tilePosition = tilemap.WorldToCell(worldPosition);
                if (highlightedTilePositions.Contains(tilePosition) && tilePosition != selectedHero.CurrentTilePosition)
                {
                    selectedHero.transform.position = tilePosition + new Vector3(0.5f, 0.5f, 0);
                    selectedHero.MoveHero();
                }
            }
        }
    }

    #region  Player 관련
    void HighlightMovableTiles()
    {
        // 마우스 위치에서 타일을 가져옴
        Vector3Int mousePosition = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        // 이전 위치로 가지 않음
        if (mousePosition != PrevHighlightedPosition)
        {
            // 현재 위치와도 다른 곳이며, stepCount보다 낮을때(기본으로 원래 위치가 들어가서 +1)
            if (mousePosition != NowHighlightedPosition && highlightedTilePositions.Count < selectedHero.step + 1)
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

                    showAttackRange(NowHighlightedPosition, selectedHero.attackRange, selectedHero.isAtkRangeInternal);
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


            showAttackRange(NowHighlightedPosition, selectedHero.attackRange, selectedHero.isAtkRangeInternal);
        }

        lineRenderer.positionCount = highlightedTilePositions.Count;
        lineRenderer.SetPositions(ConvertStackToList(highlightedTilePositions).ToArray());
    }

    bool CanGo(Vector3Int position)
    {
        /*
        * 1. 제일 마지막 방문 위치의 4방향(상, 하, 좌, 우)에 있어야 함
        * 2. 적이나 캐릭터가 없어야함
        * 3. 한번 갔던 곳이면 안됨
        * 4. 타일이 없는 곳이면 안됨
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
        bool isNoEnemy;
        // 타일 좌표 월드로 계산
        Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
        // AttackRange에 가려지지 않게 레이어마스크 추가해서 확인
        Collider2D collider = Physics2D.OverlapPoint(worldPosition, 1 << LayerMask.NameToLayer("Character"));
        isNoEnemy = collider == null;

        // 3번 조건
        bool isNoCameTile = !highlightedTilePositions.Contains(position);

        // 4번 조건
        bool isHasTile = tilemap.HasTile(position);

        return isLinked && isNoEnemy && isNoCameTile && isHasTile;
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

    public void RemoveAllAttackRange()
    {
        // 일단 다 지우기 (오브젝트 풀 형식으로 변경)
        List<GameObject> list = GameObject.FindGameObjectsWithTag("AttackRange").ToList();
        foreach (GameObject go in list)
        {
            Destroy(go);
        }
    }
    #endregion

    #region Enemy 관련

    public Vector3Int GetEnemyRoute(Vector3Int start, Vector3Int goal, int step)
    {
        List<Vector3Int> path = FindPath(start, goal);

        Vector3Int targetPosition = start;
        tilemap.SetTile(targetPosition, highlightedTile);
        if (path.Count > 0)
        {
            for (int i = 0; i < Mathf.Min(step, path.Count - 1); i++)
            {
                targetPosition = path[i];
                tilemap.SetTile(targetPosition, highlightedTile);
                highlightedTilePositions.Push(targetPosition);
            }
        }


        showAttackRange(targetPosition, selectedEnemy.attackRange, selectedEnemy.isAtkRangeInternal);
        return targetPosition;
    }

    List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        // A* 알고리즘을 구현합니다.
        List<Vector3Int> openSet = new List<Vector3Int> { start };
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        Dictionary<Vector3Int, int> gScore = new Dictionary<Vector3Int, int>();
        gScore[start] = 0;

        Dictionary<Vector3Int, int> fScore = new Dictionary<Vector3Int, int>();
        fScore[start] = Heuristic(start, goal);

        while (openSet.Count > 0)
        {
            Vector3Int current = GetLowestFScore(openSet, fScore);

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                // 해당 위치에 적이나 플레이어가 없어야 함
                Vector3 worldPosition = tilemap.GetCellCenterWorld(neighbor);
                Collider2D collider = Physics2D.OverlapPoint(worldPosition, 1 << LayerMask.NameToLayer("Character"));

                if (collider != null && neighbor != goal)
                    continue;

                int tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // 목표에 도달하지 못했을 경우, 가능한 최선의 경로를 반환합니다.
        Vector3Int closest = GetClosestPosition(openSet, goal);
        return ReconstructPath(cameFrom, closest);
    }

    int Heuristic(Vector3Int a, Vector3Int b)
    {
        // 맨해튼 거리 휴리스틱을 사용합니다.
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    Vector3Int GetLowestFScore(List<Vector3Int> openSet, Dictionary<Vector3Int, int> fScore)
    {
        Vector3Int lowest = openSet[0];
        foreach (Vector3Int pos in openSet)
        {
            if (fScore[pos] < fScore[lowest])
            {
                lowest = pos;
            }
        }
        return lowest;
    }

    Vector3Int GetClosestPosition(List<Vector3Int> openSet, Vector3Int goal)
    {
        Vector3Int closest = openSet[0];
        int minDistance = Heuristic(openSet[0], goal);

        foreach (Vector3Int pos in openSet)
        {
            int distance = Heuristic(pos, goal);
            if (distance < minDistance)
            {
                closest = pos;
                minDistance = distance;
            }
        }

        return closest;
    }

    List<Vector3Int> GetNeighbors(Vector3Int pos)
    {
        // 상하좌우 이웃을 반환합니다.
        List<Vector3Int> neighbors = new List<Vector3Int>
        {
            new Vector3Int(pos.x + 1, pos.y, pos.z),
            new Vector3Int(pos.x - 1, pos.y, pos.z),
            new Vector3Int(pos.x, pos.y + 1, pos.z),
            new Vector3Int(pos.x, pos.y - 1, pos.z)
        };

        // 타일맵 범위 내에 있는 이웃만 반환합니다.
        neighbors.RemoveAll(n => !tilemap.HasTile(n));
        return neighbors;
    }

    List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> totalPath = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }

        return totalPath;
    }

    #endregion

    #region AttackRange

    void showAttackRange(Vector3Int target, int range, bool isInternal)
    {
        RemoveAllAttackRange();

        // 범위 내의 모든 좌표를 반복합니다.
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int position = new Vector3Int(target.x + x, target.y + y, target.z);

                if (!isInternal)
                {
                    // 맨해튼 거리가 maxSteps 이하인 좌표만 고려합니다.
                    if (Mathf.Abs(x) + Mathf.Abs(y) == range && tilemap.HasTile(position))
                    {
                        Instantiate(attackRange, ConvertTileToWorldPosition(position), Quaternion.identity);
                    }
                }
                else
                {
                    // 맨해튼 거리가 maxSteps 이하인 좌표만 고려합니다.
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= range && tilemap.HasTile(position))
                    {
                        Instantiate(attackRange, ConvertTileToWorldPosition(position), Quaternion.identity);
                    }
                }
            }
        }
    }

    #endregion



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

    public Vector3 ConvertTileToWorldPosition(Vector3Int pos)
    {
        return new Vector3(pos.x + 0.5f, pos.y + 0.5f, pos.z);
    }

    public Vector3Int GetCurrentTilePosition(Transform tf)
    {
        return tilemap.WorldToCell(tf.position);
    }

}
