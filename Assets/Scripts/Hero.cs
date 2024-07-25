using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hero : MonoBehaviour
{
    [Header("정보")]
    public int StepCount = 4;


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

    void OnMouseDown()
    {
        if (!isSelected)
        {
            isSelected = true;

            gridHighlighter.selectedHero = this;
            gridHighlighter.PrevHighlightedPosition = CurrentTilePosition;
            gridHighlighter.HighlightSpecificTile(CurrentTilePosition);
        }
        else
        {
            isSelected = false;

            gridHighlighter.selectedHero = null;
            gridHighlighter.HighlightSpecificTile(CurrentTilePosition);
            gridHighlighter.UnHighlightSpecificTile(CurrentTilePosition);
        }
    }

    Vector3Int GetCurrentTilePosition()
    {
        return tilemap.WorldToCell(transform.position);
    }
}
