using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : Character
{

    [Header("UI")]
    EnemyUI EnemyUI;


    List<Hero> heroes = new List<Hero>();

    private Vector3Int enemyPosition;
    // Position은 기술의 사거리에 따라 달라짐(tragetplayer의 위치 != playerPosition)
    private Vector3Int playerPosition;
    public Hero targetPlayer;

    [SerializeField] StateUI stateUI;

    protected override void Awake()
    {
        base.Awake();

        heroes = FindObjectsOfType<Hero>().ToList();

        stateUI = FindObjectOfType<StateUI>();
    }

    void Start()
    {
        tilemap = GridHighlighter.Instance.tilemap;
        transform.position = GridHighlighter.Instance.ConvertTileToWorldPosition(tilemap.WorldToCell(transform.position));

        // 대충 놔도 스냅되도록
        transform.position = GridHighlighter.Instance.ConvertTileToWorldPosition(tilemap.WorldToCell(transform.position));

        enemyPosition = tilemap.WorldToCell(transform.position);

        EnemyUI = Instantiate(CharacterUIPfb, transform.position + CharacterUIPositionOffset, Quaternion.identity, GameObject.Find("CharacterUIs").transform).GetComponent<EnemyUI>();
        EnemyUI.Init(this);
    }

    protected override void Update()
    {
        base.Update();

        EnemyUI.transform.position = transform.position + CharacterUIPositionOffset;
    }

    void OnMouseOver()
    {
        stateUI.ShowStat(this);

        if (TurnManager.Instance.nowTurnCharacter is Hero)
        {
            UIManager.Instance.ShowAccDmgInfo(TurnManager.Instance.nowTurnCharacter as Hero, this);
        }
    }

    void OnMouseExit()
    {
        stateUI.hideStat();

        if (TurnManager.Instance.nowTurnCharacter is Hero)
        {
            UIManager.Instance.HideAccDmgInfo();
        }
    }

    protected override void OperateCharacter()
    {
        enemyPosition = tilemap.WorldToCell(transform.position);

        // targetPlayer는 지정하면서 사거리 기준 위치 잡아주기
        playerPosition = FindClosestPlayer();

        GridHighlighter.Instance.selectedEnemy = this;
        Vector3Int targetPosition = GridHighlighter.Instance.GetEnemyRoute(enemyPosition, playerPosition, step);

        StartCoroutine(waitForMoving(targetPosition));
    }

    public override void removeTurnUI()
    {
        EnemyUI.removeTurnUI();
    }

    public override void createTurnUI()
    {
        EnemyUI.createTurnUI();
    }

    /*
    1. 가장 가까이에 있는 플레이어를 찾음
    2. 여러개라면, 현재 체력이 가장 약한 애 찾음
    3. 현재는 단순 거리 비교 > 나중에 못가는 타일도 포함한 길찾기 알고리즘으로 수정
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

                    // 맨해튼 거리 계산
                    int distance = Mathf.Abs(enemyPosition.x - targetPosition.x) + Mathf.Abs(enemyPosition.y - targetPosition.y);


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
        GridHighlighter.Instance.UnHighlightAllTile();
        Attack();
    }

    public override void OnDamaged(int damage, bool isCritical, bool isEffect = true)
    {
        base.OnDamaged(damage, isCritical, isEffect);

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Attack()
    {
        List<Character> targets = new List<Character>();
        List<AttackRange> ranges = FindObjectsOfType<AttackRange>().ToList();

        // 힐이 아니라면 공격으로 간주 -> 히어로 파악
        foreach (AttackRange go in ranges)
        {
            if (go.hero != null)
            {
                targets.Add(go.hero);
            }
        }

        if (targets.Count == 0)
        {
            // 턴 넘길 준비
            GoToNextTurn();
        }
        else
        {
            // 적 하나 이상 발견
            if (targets.Count > 1)
            {
                // 단일 공격이라면 하나 선택
                if (equippedTech.TechTarget == TechTarget.Single)
                {
                    ChooseCharacter(targets);
                }
                // 다중 공격이라면 모두에게 적용
                else
                {
                    CombatManager.Instance.Combat(this, targets);
                }
            }
            // 적 하나 발견
            else
            {
                if (targets.Count > 0)
                {
                    CombatManager.Instance.Combat(this, targets[0]);
                }
            }

        }

        GridHighlighter.Instance.RemoveAllAttackRange();
    }

    void ChooseCharacter(List<Character> characters)
    {
        // 일단 가장 체력 낮은 애
        // 더 발전하면 현재 공격 타입에 따라서 제일 약한 애

        Hero hero = null;
        foreach (Character c in characters)
        {
            Hero h = c.GetComponent<Hero>();

            if (h != null)
            {
                if (hero == null)
                    hero = h;
                else
                {
                    if (h.hp < hero.hp)
                    {
                        hero = h;
                    }
                }
            }
        }

        CombatManager.Instance.Combat(this, hero);

        GridHighlighter.Instance.UnHighlightAllTile();
    }
}
