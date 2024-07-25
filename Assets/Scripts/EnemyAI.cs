using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour
{
    GridHighlighter gridHighlighter;
    Tilemap tilemap;

    public int step = 3; // Enemy의 최대 이동 거리
    public int attackRange = 1; // 공격 범위

    List<Hero> heroes = new List<Hero>();

    private Vector3Int enemyPosition;
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
        targetPlayer = FindClosestPlayer();
        playerPosition = tilemap.WorldToCell(targetPlayer.transform.position);

        gridHighlighter.UnHighlightAllTile();
        gridHighlighter.ShowEnemyRoute(enemyPosition, playerPosition, step);
    }

    /*
    1. 가장 가까이에 있는 플레이어를 찾음
    2. 여러개라면, 현재 체력이 가장 약한 애 찾음
    */
    Hero FindClosestPlayer()
    {
        Hero closestPlayer = null;
        float minDistance = float.MaxValue;
        int lowestHealth = int.MaxValue;

        foreach (var player in heroes)
        {
            if (player != null)
            {
                Vector3Int playerPosition = tilemap.WorldToCell(player.transform.position);
                float distance = Vector3Int.Distance(enemyPosition, playerPosition);

                int playerHealth = player.hp;

                if (distance < minDistance || (distance == minDistance && playerHealth < lowestHealth))
                {
                    minDistance = distance;
                    lowestHealth = playerHealth;
                    closestPlayer = player;
                }
            }
        }

        return closestPlayer;
    }

    void Attack()
    {
        // 플레이어를 공격하는 로직을 구현합니다.
        Debug.Log("Enemy attacks Player!");
    }
}
