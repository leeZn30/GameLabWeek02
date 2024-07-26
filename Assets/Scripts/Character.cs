using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Character : MonoBehaviour
{
    [Header("능력치")]
    public CharacterData characterData;
    public int step => characterData.Step;
    public float crit => characterData.Crit;

    [Header("기술")]
    int techIndex = 0;
    public TechData equippedTech => characterData.Techs[techIndex];
    public int attackRange => characterData.Techs[techIndex].Range;
    public bool isAtkRangeInternal => characterData.Techs[techIndex].isInternal;

    [Header("현재 상태")]
    public int hp;
    // 0: default 1: Awakening 2: Collapse
    public int StressState = 0;

    [Header("타일맵")]
    protected Tilemap tilemap;

    protected virtual void Awake()
    {
        hp = characterData.MaxHp;

        tilemap = GridHighlighter.Instance.tilemap;

        // 대충 놔도 스냅되도록
        transform.position = GridHighlighter.Instance.ConvertTileToWorldPosition(tilemap.WorldToCell(transform.position));
    }

    // 크리티컬이면 스트레스 10 증가
    public void OnDamaged(int damage, bool isCritical)
    {
        hp -= damage;

        if (hp < 0)
        {
            Debug.Log("죽음의 문턱");
        }

        if (isCritical)
        {
            OnStressed(10);

            // 주변 2칸 아군(플레이어) 50% 확률로 스트레스
            foreach (Hero hero in GetNearHeroes<Hero>(2))
            {
                if (Random.Range(0, 101) <= 50)
                {
                    hero.OnStressed(5);
                }
            }
        }
    }

    // Hero에서만 구현되어야 함
    public virtual void OnStressed(int stress) { }

    public void OnHealed(int heal)
    {
        hp += heal;

        if (hp > characterData.MaxHp)
        {
            hp = characterData.MaxHp;
        }
    }

    public void OnStun()
    {

    }

    public void OnBleed(int damage, int turnCnt)
    {

    }

    public void OnPoison(int damage, int turnCnt)
    {

    }

    List<T> GetNearHeroes<T>(int blockCnt)
    {
        List<T> results = new List<T>();

        Vector3Int target = tilemap.WorldToCell(transform.position);

        // 주변 blockCnt 안에 존재하는 T찾기
        for (int x = -blockCnt; x <= blockCnt; x++)
        {
            for (int y = -blockCnt; y <= blockCnt; y++)
            {
                Vector3Int position = new Vector3Int(target.x + x, target.y + y, target.z);

                // 맨해튼 거리가 maxSteps 이하인 좌표만 고려합니다.
                if (Mathf.Abs(x) + Mathf.Abs(y) <= blockCnt)
                {
                    Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
                    Collider2D coll = Physics2D.OverlapPoint(worldPosition, 1 << LayerMask.NameToLayer("Character"));
                    if (coll != null && coll.GetComponent<T>() != null)
                    {
                        results.Add(coll.GetComponent<T>());
                    }
                }
            }
        }


        return results;
    }

}
