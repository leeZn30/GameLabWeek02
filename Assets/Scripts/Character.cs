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

    [Header("UI")]
    [SerializeField] protected GameObject DescUIGridPfb;
    [SerializeField] protected GameObject DescUIPfb;
    protected GameObject DescGrid;
    // float DescGridYOffset = transform.localScale.y / 2 + DescUIGridPfb.transform.localScale.y / 2;
    Vector3 DescGridPositionOffset;

    [Header("Effect")]
    [SerializeField] GameObject atk;
    [SerializeField] GameObject stress;
    [SerializeField] GameObject heal;
    [SerializeField] GameObject stressheal;

    protected virtual void Awake()
    {
        hp = characterData.MaxHp;

        tilemap = GridHighlighter.Instance.tilemap;

        // 대충 놔도 스냅되도록
        transform.position = GridHighlighter.Instance.ConvertTileToWorldPosition(tilemap.WorldToCell(transform.position));
        DescGridPositionOffset = new Vector3(0, transform.localScale.y / 2, 0);
        DescGrid = Instantiate(DescUIGridPfb, transform.position + DescGridPositionOffset, Quaternion.identity, GameObject.Find("Canvas").transform);
    }

    protected virtual void Update()
    {
        // DescGrid 계속 따라다니게 하기
        DescGrid.transform.position = transform.position + DescGridPositionOffset;

        // 모든 descUI가 없으면 끝난 것 > 카메라 줌 아웃
        if (FindObjectsOfType<CharacterDescUI>().Length == 0 && Camera.main.orthographicSize != 8)
        {
            CombatManager.Instance.CallZoomOutCamera();
        }
    }

    public virtual void OnDamaged(int damage, bool isCritical)
    {
        TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
        if (isCritical)
        {
            desc.SetText(string.Format("<color=red>치명타! {0}", damage));
        }
        else
        {
            desc.SetText(string.Format("<color=red>{0}", damage));
        }
        StartCoroutine(damaged());

        hp -= damage;
    }

    // Hero에서만 구현되어야 함
    public virtual void OnStressed(int stress, bool isCritical, bool isEffect = true) { }

    // Hero에서만 구현되어야 함
    public virtual void OnDidCritical() { }

    public void OnHealed(int heal, bool isCritical)
    {
        TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
        if (isCritical)
            desc.SetText(string.Format("<color=#9BFF00>치명타! {0}", heal));
        else
            desc.SetText(string.Format("<color=#9BFF00>{0}", heal));

        StartCoroutine(healed());

        hp += heal;

        if (hp > characterData.MaxHp)
        {
            hp = characterData.MaxHp;
        }
    }

    // Hero에서만 구현되어야 함
    public virtual void OnStressHealed(int heal, bool isCritical, bool isEffect = false)
    {
    }

    public void OnStun(bool isStun, bool isStunEnable)
    {
        if (isStunEnable)
        {
            TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();

            if (isStun)
            {
                desc.SetText("<color=yellow>기절");
            }
            else
            {
                desc.SetText("<color=yellow>기절 저항");
            }
        }
    }

    public void OnBleed(bool isBleed, bool isBleedEnable, int damage = 0, int turnCnt = 0)
    {
        if (isBleedEnable)
        {
            TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();

            if (isBleed)
            {
                desc.SetText("<color=#C100A5>출혈");
            }
            else
            {
                desc.SetText("<color=#C100A5>출혈 저항");
            }
        }
    }

    public void OnPoison(bool isPoison, bool isPoisonEnable, int damage = 0, int turnCnt = 0)
    {
        if (isPoisonEnable)
        {
            TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();

            if (isPoison)
            {
                desc.SetText("<color=green>중독");
            }
            else
            {
                desc.SetText("<color=green>중독 저항");
            }
        }
    }

    public void OnDodged(TechType techType)
    {
        TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
        desc.SetText("회피!");
        switch (techType)
        {
            case TechType.Attack:
                StartCoroutine(damaged());
                break;

            case TechType.Stress:
                StartCoroutine(stressed());
                break;
        }

        StartCoroutine(dodged());
    }

    protected List<T> GetNearHeroes<T>(int blockCnt)
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
                    if (coll != null && coll.GetComponent<T>() != null && coll.gameObject != gameObject)
                    {
                        results.Add(coll.GetComponent<T>());
                    }
                }
            }
        }
        return results;
    }


    IEnumerator damaged()
    {
        GameObject go = Instantiate(atk, transform.position, atk.transform.rotation);

        yield return new WaitForSeconds(2f);

        Destroy(go);
    }

    protected IEnumerator stressed()
    {
        GameObject go = Instantiate(stress, transform.position, stress.transform.rotation);

        yield return new WaitForSeconds(2f);

        Destroy(go);
    }

    IEnumerator healed()
    {
        GameObject go = Instantiate(heal, transform.position, heal.transform.rotation);

        yield return new WaitForSeconds(2f);

        Destroy(go);
    }

    protected IEnumerator stresshealed()
    {
        GameObject go = Instantiate(stressheal, transform.position, stressheal.transform.rotation);

        yield return new WaitForSeconds(2f);

        Destroy(go);
    }

    IEnumerator dodged()
    {
        Vector3 originPose = transform.position;
        Vector3 endPose = transform.position + new Vector3(1.5f, 0, 0);


        float duration = 0.3f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / duration);

            // 위치 보간 (Lerp를 사용하여 대각선으로 자연스럽게 이동)
            transform.position = Vector3.Lerp(originPose, endPose, t);
            yield return null;
        }

        yield return new WaitForSeconds(1.7f);

        transform.position = originPose;
    }

}
