using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hero : MonoBehaviour
{
    [Header("정보")]
    public HeroData heroData;
    public int hp => heroData.Hp;
    public int step => heroData.Step;


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
        gridHighlighter.playerTiles.Add(CurrentTilePosition);
    }

    void Update()
    {
        // 임시
        if (Input.GetKeyDown(KeyCode.Escape) && isSelected)
        {
            isSelected = false;

            gridHighlighter.selectedHero = null;
            gridHighlighter.UnHighlightAllTile();
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

    Vector3Int GetCurrentTilePosition()
    {
        return tilemap.WorldToCell(transform.position);
    }

    public void UnselectHero()
    {
        CurrentTilePosition = GetCurrentTilePosition();

        isSelected = false;

        gridHighlighter.selectedHero = null;
        gridHighlighter.UnHighlightAllTile();
    }
}
