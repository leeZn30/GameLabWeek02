using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hero : Character
{
    [Header("상태")]
    [SerializeField] bool isSelected;
    public Vector3Int CurrentTilePosition;
    bool isChoosing;

    GridHighlighter gridHighlighter;

    protected override void Awake()
    {
        base.Awake();

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

        if (isChoosing && Input.GetMouseButton(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.down, 1f, 1 << LayerMask.NameToLayer("Character"));

            if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            {
                CombatManager.Instance.Combat(this, hit.collider.GetComponent<EnemyAI>());
                gridHighlighter.RemoveAllAttackRange();

                isChoosing = false;

                UIManager.Instance.HideGameInfo();
            }
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
        List<Character> targets = new List<Character>();
        List<AttackRange> ranges = FindObjectsOfType<AttackRange>().ToList();

        // 힐이 아니라면 공격으로 간주 -> 에너미 파악
        if (equippedTech.TechType != TechType.Heal)
        {
            foreach (AttackRange go in ranges)
            {
                if (go.enemy != null)
                {
                    targets.Add(go.enemy);
                }
            }

            // 적 하나 이상 발견
            if (targets.Count > 1)
            {
                // 단일 공격이라면 하나 선택
                if (equippedTech.TechTarget == TechTarget.Single)
                {
                    ReadyToChooseEnemy();
                }
                // 다중 공격이라면 모두에게 적용
                else
                {
                    CombatManager.Instance.Combat(this, targets);
                    gridHighlighter.RemoveAllAttackRange();
                }
            }
            // 적 하나 발견
            else
            {
                if (targets.Count > 0)
                {
                    CombatManager.Instance.Combat(this, targets[0]);
                    gridHighlighter.RemoveAllAttackRange();
                }
            }
        }
        // 힐이면 -> 플레이어 파악
        else
        {
            foreach (AttackRange go in ranges)
            {
                if (go.hero != null && go.hero != this)
                {
                    targets.Add(go.hero);
                    Debug.Log(go.hero);
                }
            }

            // 플레이어 하나 이상 발견
            if (targets.Count > 1)
            {
                // 단일 힐이라면 하나 선택
                if (equippedTech.TechTarget == TechTarget.Single)
                {
                    ReadyToChooseEnemy();
                }
                // 다중 힐이라면 모두에게 적용
                else
                {
                    CombatManager.Instance.Combat(this, targets);
                    gridHighlighter.RemoveAllAttackRange();
                }
            }
            // 플레이어 하나 발견
            else
            {
                if (targets.Count > 0)
                {
                    CombatManager.Instance.Combat(this, targets[0]);
                    gridHighlighter.RemoveAllAttackRange();
                }
            }
        }

        // 이동만 한 것
        if (targets.Count == 0)
            gridHighlighter.RemoveAllAttackRange();
    }

    public override void OnStressed(int stress)
    {
        characterData.Stress += stress;

        ChangeStressState();
    }

    void ChangeStressState()
    {
        // 각성/붕괴 결정
        if (characterData.Stress >= 100 && StressState == 0)
        {
            if (Random.Range(0, 101) < characterData.WillPower)
            {
                // 각성
                StressState = 1;
                Debug.Log("각성!");
            }
            else
            {
                // 붕괴
                StressState = 2;
                Debug.Log("붕괴!");
            }
        }
        // 사망
        else if (characterData.Stress >= 200)
        {
            Debug.Log("심장마비!");
        }
    }

    void ReadyToChooseEnemy()
    {
        if (!isChoosing)
            isChoosing = true;

        // UI 뜨게
        string text = "단일 공격입니다.\n공격하고 싶은 적을 클릭하세요.";
        UIManager.Instance.ShowGameInfo(text);
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
