using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hero : MonoBehaviour
{
    [Header("정보")]
    public CharacterData heroData;
    public int hp => heroData.Hp;
    public int step => heroData.Step;
    int techIndex = 0;
    public TechData equippedTech => heroData.Techs[techIndex];
    public int attackRange => heroData.Techs[techIndex].Range;
    public bool isAtkRangeInternal => heroData.Techs[techIndex].isInternal;


    [Header("상태")]
    [SerializeField] bool isSelected;
    public Vector3Int CurrentTilePosition;

    GridHighlighter gridHighlighter;
    Tilemap tilemap;

    void Awake()
    {
        gridHighlighter = FindObjectOfType<GridHighlighter>();
        tilemap = gridHighlighter.GetComponent<Tilemap>();
    }

    void Start()
    {
        CurrentTilePosition = GetCurrentTilePosition();
    }

    void Update()
    {
        // 임시
        if (Input.GetKeyDown(KeyCode.Escape) && isSelected)
        {
            isSelected = false;

            gridHighlighter.selectedHero = null;
            gridHighlighter.UnHighlightAllTile();
            gridHighlighter.RemoveAllAttackRange();
        }
    }

    void OnMouseDown()
    {
        if (!isSelected)
        {
            isSelected = true;

            gridHighlighter.selectedHero = this;
            gridHighlighter.HighlightStartTile(CurrentTilePosition);
        }
        else
        {
            isSelected = false;

            gridHighlighter.selectedHero = null;
            gridHighlighter.UnHighlightAllTile();
        }

    }

    void Attack()
    {
        List<EnemyAI> enemies = new List<EnemyAI>();

        // 범위 내의 모든 좌표를 반복합니다.
        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                Vector3Int position = new Vector3Int((int)transform.position.x + x, (int)transform.position.y + y, (int)transform.position.z);

                if (!isAtkRangeInternal)
                {
                    // 맨해튼 거리가 maxSteps 이하인 좌표만 고려합니다.
                    if (Mathf.Abs(x) + Mathf.Abs(y) == attackRange)
                    {
                        Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
                        Collider2D collider = Physics2D.OverlapPoint(worldPosition);
                        if (collider != null && collider.CompareTag("Enemy"))
                            enemies.Add(collider.GetComponent<EnemyAI>());
                    }
                }
                else
                {
                    // 맨해튼 거리가 maxSteps 이하인 좌표만 고려합니다.
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= attackRange)
                    {
                        Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
                        Collider2D collider = Physics2D.OverlapPoint(worldPosition);
                        if (collider != null && collider.CompareTag("Enemy"))
                            enemies.Add(collider.GetComponent<EnemyAI>());
                    }
                }
            }
        }

        if (enemies.Count > 0)
        {
            enemies[0].GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }

    Vector3Int GetCurrentTilePosition()
    {
        return tilemap.WorldToCell(transform.position);
    }

    public void MoveHero()
    {
        CurrentTilePosition = GetCurrentTilePosition();

        isSelected = false;

        gridHighlighter.selectedHero = null;
        gridHighlighter.RemoveAllAttackRange();
        gridHighlighter.UnHighlightAllTile();

        Attack();
    }
}
