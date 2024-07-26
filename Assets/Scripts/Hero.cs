using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            gridHighlighter.RemoveAllAttackRange();
        }

    }

    void Attack()
    {
        List<EnemyAI> enemies = new List<EnemyAI>();

        List<AttackRange> ranges = FindObjectsOfType<AttackRange>().ToList();
        foreach (AttackRange go in ranges)
        {
            if (go.enemy != null)
                go.enemy.GetComponent<SpriteRenderer>().color = Color.gray;
        }

        gridHighlighter.RemoveAllAttackRange();
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
        gridHighlighter.UnHighlightAllTile();

        Attack();
    }
}
