using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class StatusAbnormal
{
    public int damage;
    public int turns;

    public StatusAbnormal(int dmg, int turns)
    {
        this.damage = dmg;
        this.turns = turns;
    }
}

public class Character : MonoBehaviour
{
    [Header("능력치")]
    public CharacterData characterData;
    public int step => characterData.Step;
    public float crit => characterData.Crit;

    [Header("기술")]
    public int techIndex = 0;
    public TechData equippedTech => characterData.Techs[techIndex];
    public int attackRange => characterData.Techs[techIndex].Range;
    public bool isAtkRangeInternal => characterData.Techs[techIndex].isInternal;

    [Header("현재 상태")]
    public int hp;
    public int stress;
    // 0: default 1: Awakening 2: Collapse
    public int StressState = 0;
    public int nowSpeed;
    public bool isStun;
    List<StatusAbnormal> bleedStatus = new List<StatusAbnormal>();
    public bool isBleeding => bleedStatus.Count > 0;
    List<StatusAbnormal> poisonStatus = new List<StatusAbnormal>();
    public bool isPoisoning => poisonStatus.Count > 0;
    public bool myTurn = false;
    protected bool isSkipTurn;

    [Header("타일맵")]
    protected Tilemap tilemap;

    [Header("UI")]
    [SerializeField] protected GameObject CharacterUIPfb;
    protected GameObject CharacterUI;
    protected Vector3 CharacterUIPositionOffset;
    [SerializeField] protected GameObject DescUIGridPfb;
    [SerializeField] protected GameObject DescUIPfb;
    protected GameObject DescGrid;
    Vector3 DescGridPositionOffset;

    [Header("Effect")]
    GameObject nowEffect;
    [SerializeField] GameObject atk;
    [SerializeField] GameObject sts;
    [SerializeField] GameObject heal;
    [SerializeField] GameObject stressheal;

    [Header("오브젝트")]
    public SpriteRenderer Light;

    protected virtual void Awake()
    {
        hp = characterData.MaxHp;
        stress = characterData.Stress;

        CharacterUIPositionOffset = new Vector3(0, transform.localScale.y / 2, 0);
        DescGridPositionOffset = new Vector3(0, transform.localScale.y / 2 + CharacterUIPfb.transform.localScale.y / 2, 0);
        DescGrid = Instantiate(DescUIGridPfb, transform.position + DescGridPositionOffset, Quaternion.identity, GameObject.Find("Canvas").transform);

        Light.gameObject.SetActive(false);
    }

    protected virtual void Update()
    {
        // DescGrid 계속 따라다니게 하기
        DescGrid.transform.position = transform.position + DescGridPositionOffset;

        // 모든 descUI가 없으면 끝난 것 > 카메라 줌 아웃
        if (myTurn)
        {
            if (FindObjectsOfType<CharacterDescUI>().Length == 0 && Camera.main.orthographicSize == 4)
            {
                CombatManager.Instance.CallZoomOutCamera();
            }
        }
    }

    public virtual void StartTurn()
    {
        StartCoroutine(turnProcedure());
    }

    public virtual void removeTurnUI() { }
    public virtual void createTurnUI() { }

    void OnDestroy()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.removeCharacterFromQueue(this);
        }

        Destroy(nowEffect);
    }

    IEnumerator turnProcedure()
    {
        // 개복잡

        // 상태 이상 공격 데미지
        if (bleedStatus.Count > 0)
        {
            int sumBleed = 0;
            for (int i = 0; i < bleedStatus.Count; i++)
            {
                sumBleed += bleedStatus[i].damage;
                bleedStatus[i].turns--;

                if (bleedStatus[i].turns == 0)
                {
                    bleedStatus.RemoveAt(i);
                }
            }

            TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
            desc.SetText("<color=#C100A5>출혈 상태");

            OnDamaged(sumBleed, false, false, true);

            yield return new WaitForSeconds(1f);
        }

        // 출혈로 죽으면 isSkipTurn 활성화
        if (isSkipTurn)
        {
            TurnManager.Instance.StartNextTurn();
            Destroy(gameObject);
        }
        else
        {
            if (poisonStatus.Count > 0)
            {
                int sumPoison = 0;
                for (int i = 0; i < poisonStatus.Count; i++)
                {
                    sumPoison += poisonStatus[i].damage;
                    poisonStatus[i].turns--;

                    if (poisonStatus[i].turns == 0)
                    {
                        poisonStatus.RemoveAt(i);
                    }
                }

                TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                desc.SetText("<color=green>중독 상태");

                OnDamaged(sumPoison, false, false, true);

                yield return new WaitForSeconds(1f);
            }

            // 중독으로 죽으면 isSkipTurn 활성화
            if (isSkipTurn)
            {
                TurnManager.Instance.StartNextTurn();
                Destroy(gameObject);
            }
            else
            {
                // 기절이 아니라면 각성/붕괴에 따른 효과 후, 이동 및 공격 시작
                if (!isStun)
                {
                    // 각성
                    if (StressState == 1)
                    {
                        DoAwakening();
                        yield return new WaitForSeconds(2f);
                    }
                    // 붕괴
                    else if (StressState == 2)
                    {
                        DoCollapse();
                        yield return new WaitForSeconds(2f);
                    }

                    myTurn = true;

                    if (!isSkipTurn)
                    {
                        // 이동 및 공격 시작
                        OperateCharacter();
                    }
                    else
                    {
                        isSkipTurn = true;
                        TurnManager.Instance.StartNextTurn();
                    }
                }
                // 기절 깨우기
                else
                {
                    TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                    desc.SetText("<color=yellow> 기절 회복");
                    isStun = false;

                    yield return new WaitForSeconds(1f);

                    TurnManager.Instance.StartNextTurn();
                }

            }


        }

    }

    protected virtual void OperateCharacter() { }

    protected void GoToNextTurn()
    {
        if (TurnManager.Instance != null)
            TurnManager.Instance.StartNextTurn();
    }

    // Hero에서 구현
    public virtual void DoAwakening() { }
    // Hero에서 구현
    public virtual void DoCollapse() { }

    public virtual void OnDamaged(int damage, bool isCritical, bool isEffect = true, bool isStatusAbnormal = false)
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

        if (isEffect)
            StartCoroutine(damaged());

        hp -= damage;
    }

    // Hero에서만 구현되어야 함
    public virtual void OnStressed(int stress, bool isCritical, bool isEffect = true) { }

    // Hero에서만 구현되어야 함
    public virtual void OnDidCritical() { }

    // hero에서만 구현되어야 함
    public virtual void OnHealed(int heal, bool isCritical) { }

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
                this.isStun = true;
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
                bleedStatus.Add(new StatusAbnormal(damage, turnCnt));
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
                poisonStatus.Add(new StatusAbnormal(damage, turnCnt));
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

    public Tuple<int, int> GetBleedState()
    {
        int sumBleed = 0;
        int maxTurn = -1;
        for (int i = 0; i < bleedStatus.Count; i++)
        {
            sumBleed += bleedStatus[i].damage;
            if (bleedStatus[i].turns > maxTurn)
            {
                maxTurn = bleedStatus[i].turns;
            }

        }

        return new Tuple<int, int>(sumBleed, maxTurn);
    }

    public Tuple<int, int> GetPoisonState()
    {
        int sumPoison = 0;
        int maxTurn = -1;
        for (int i = 0; i < poisonStatus.Count; i++)
        {
            sumPoison += poisonStatus[i].damage;
            if (poisonStatus[i].turns > maxTurn)
            {
                maxTurn = poisonStatus[i].turns;
            }
        }

        return new Tuple<int, int>(sumPoison, maxTurn);
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
        nowEffect = Instantiate(atk, transform.position, atk.transform.rotation);

        yield return new WaitForSeconds(2f);

        Destroy(nowEffect);
    }

    protected IEnumerator stressed()
    {
        nowEffect = Instantiate(sts, transform.position, sts.transform.rotation);

        yield return new WaitForSeconds(2f);

        Destroy(nowEffect);
    }

    protected IEnumerator healed()
    {
        nowEffect = Instantiate(heal, transform.position, heal.transform.rotation);

        yield return new WaitForSeconds(2f);

        Destroy(nowEffect);
    }

    protected IEnumerator stresshealed()
    {
        nowEffect = Instantiate(stressheal, transform.position, stressheal.transform.rotation);

        yield return new WaitForSeconds(2f);

        Destroy(nowEffect);
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
