using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    [Header("정보")]
    public CharacterData enemyData;
    public int hp => enemyData.Hp;
    public int step => enemyData.Step;

    GridHighlighter gridHighlighter;
    Tilemap tilemap;

    // attackRange > 0 무조건!!
    public int attackRange = 2; // 공격 범위

    List<Hero> heroes = new List<Hero>();

    private Vector3Int enemyPosition;
    // Position은 기술의 사거리에 따라 달라짐(tragetplayer의 위치 != playerPosition)
    private Vector3Int playerPosition;
    public Hero targetPlayer;

    void Awake()
    {
        gridHighlighter = FindObjectOfType<GridHighlighter>();
        tilemap = gridHighlighter.GetComponent<Tilemap>();

        heroes = FindObjectsOfType<Hero>().ToList();
    }

    void Start()
    {
        enemyPosition = tilemap.WorldToCell(transform.position);
    }

    void Update()
    {
    }

    void OnMouseDown()
    {
        enemyPosition = tilemap.WorldToCell(transform.position);

        // targetPlayer는 지정하면서 사거리 기준 위치 잡아주기
        playerPosition = FindClosestPlayer();

        Vector3Int targetPosition = gridHighlighter.GetEnemyRoute(enemyPosition, playerPosition, step);

        StartCoroutine(waitForMoving(targetPosition));
    }

    /*
    1. 가장 가까이에 있는 플레이어를 찾음
    2. 여러개라면, 현재 체력이 가장 약한 애 찾음
    */
    Vector3Int FindClosestPlayer()
    {
        Hero closestPlayer = null;
        Vector3Int closestPosition = Vector3Int.zero;
        float minDistance = float.MaxValue;
        int lowestHealth = int.MaxValue;

        Vector2Int[] dir = new Vector2Int[]
        {
            new Vector2Int(0, 1), // 상
            new Vector2Int(0, -1), // 하
            new Vector2Int(-1, 0), // 좌
            new Vector2Int(1, 0)  // 우
        };

        foreach (var player in heroes)
        {
            if (player != null)
            {
                Vector3Int playerPosition = tilemap.WorldToCell(player.transform.position);
                int playerHealth = player.hp;

                // 공격 기술을 먼저 정함
                // 해당 기술의 attackRange 만큼 떨어진 만큼이 목표 위치임
                foreach (Vector3Int vec in dir)
                {
                    // 사거리 -1 만큼 가야함
                    Vector3Int targetPosition = playerPosition + (attackRange - 1) * vec;

                    float distance = Vector3Int.Distance(enemyPosition, targetPosition);

                    if (distance < minDistance || (distance == minDistance && playerHealth < lowestHealth))
                    {
                        minDistance = distance;
                        closestPosition = targetPosition;

                        lowestHealth = playerHealth;
                        closestPlayer = player;
                    }
                }
            }
        }

        targetPlayer = closestPlayer;
        return closestPosition;
    }

    IEnumerator waitForMoving(Vector3Int position)
    {
        yield return new WaitForSeconds(1f);

        transform.position = position + new Vector3(0.5f, 0.5f, 0);
        gridHighlighter.UnHighlightAllTile();
    }

    void Attack()
    {
        // 플레이어를 공격하는 로직을 구현합니다.
        Debug.Log("Enemy attacks Player!");
    }
}
