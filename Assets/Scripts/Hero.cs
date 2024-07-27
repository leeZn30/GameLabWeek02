using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Hero : Character
{
    [Header("상태")]
    [SerializeField] bool isSelected;
    public Vector3Int CurrentTilePosition;
    bool isChoosing;
    bool isDeathDoor;

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

    protected override void Update()
    {
        base.Update();

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
        if (equippedTech.TechType != TechType.Heal && equippedTech.TechType != TechType.StressHeal)
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

    public override void OnDamaged(int damage, bool isCritical, bool isEffect = true)
    {
        base.OnDamaged(damage, isCritical);

        // Hero에서 필요
        if (hp < 0)
        {
            if (!isDeathDoor)
            {
                // 죽음의 문턱
                hp = 0;
                isDeathDoor = true;
                UIManager.Instance.ShowGameInfo(string.Format("{0}이 죽음의 문턱에 섰습니다.\n회복 없이 이후 공격 받은면, 바로 죽을 수도 있습니다."));
            }
            else
            {
                if (Random.Range(0, 101) <= characterData.DeathResist)
                {
                    hp = 0;
                    TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                    desc.SetText("<color=#721420>죽음 저항");
                }
                else
                {
                    TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                    desc.SetText("<color=#721420>사망");
                    Destroy(gameObject);
                }

            }
        }

        if (isCritical)
        {
            OnStressed(10, false, false);

            // 주변 2칸 아군(플레이어) 50% 확률로 스트레스
            foreach (Hero hero in GetNearHeroes<Hero>(2))
            {
                if (Random.Range(0, 101) <= 50)
                {
                    hero.OnStressed(5, false, false);
                }
            }
        }
    }

    public override void OnStressed(int stress, bool isCritical, bool isEffect = true)
    {
        TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
        if (isCritical)
            desc.SetText(string.Format("<color=black>치명타!\n{0}", stress));
        else
            desc.SetText(string.Format("<color=black>{0}", stress));

        if (isEffect)
            StartCoroutine(stressed());

        characterData.Stress += stress;

        ChangeStressState();
    }

    public override void OnHealed(int heal, bool isCritical)
    {
        TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
        if (isCritical)
            desc.SetText(string.Format("<color=#9BFF00>치명타! {0}", heal));
        else
            desc.SetText(string.Format("<color=#9BFF00>{0}", heal));

        StartCoroutine(healed());

        hp += heal;

        if (isDeathDoor)
            isDeathDoor = false;

        if (hp > characterData.MaxHp)
        {
            hp = characterData.MaxHp;
        }
    }

    public override void OnStressHealed(int heal, bool isCritical, bool isEffect = true)
    {
        TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
        if (isCritical)
            desc.SetText(string.Format("<color=white>치명타!\n{0}", heal));
        else
            desc.SetText(string.Format("<color=white>{0}", heal));
        StartCoroutine(stresshealed());

        characterData.Stress += heal;

        ChangeStressState();
    }

    public override void OnDidCritical()
    {
        // attacker는 스트레스 3 회복
        OnStressHealed(3, false);

        // 주변 2칸 이내 아군 25% 확률로 스트레스 회복
        foreach (Hero h in GetNearHeroes<Hero>(2))
        {
            if (Random.Range(0, 101) <= 25)
            {
                h.OnStressHealed(3, false, false);
            }
        }
    }

    void ChangeStressState()
    {
        string text = string.Format("{0}의 의지가 시험받고 있습니다...", characterData.ID);
        UIManager.Instance.ShowGameInfo(text);

        // 각성/붕괴 결정
        if (characterData.Stress >= 100 && StressState == 0)
        {
            if (Random.Range(0, 101) < characterData.WillPower)
            {
                // 각성
                StressState = 1;
                text = "각성!";
                UIManager.Instance.ShowGameInfo(text);
            }
            else
            {
                // 붕괴
                StressState = 2;
                text = "붕괴!";
                UIManager.Instance.ShowGameInfo(text);
            }
        }
        // 사망
        else if (characterData.Stress >= 200)
        {
            text = string.Format("{0} 심장마비", characterData.ID);
            UIManager.Instance.ShowGameInfo(text);

            Destroy(gameObject);
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


    // Hero에서 구현
    public override void DoAwakening()
    {
        int index = Random.Range(0, 3);
        TextMeshProUGUI text = null;

        switch (index)
        {
            case 0: // 스트레스 회복 전파
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("<color=white>아직 모두 할 수 있어");

                OnStressHealed(10, false);

                foreach (Hero h in GetNearHeroes<Hero>(2))
                {
                    h.OnStressHealed(5, false);
                }
                break;

            case 1: // 자힐
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("<color=#9BFF00>저에게 힘을 주소서.");

                OnHealed(5, false);
                break;

            default:
                break;
        }
    }

    // Hero에서 구현
    public override void DoCollapse()
    {
        int index = Random.Range(0, 4);
        TextMeshProUGUI text = null;

        switch (index)
        {
            case 0: // 스트레스 전파
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("<color=black>이 판은 망했어.");

                OnStressed(10, false, false);

                foreach (Hero h in GetNearHeroes<Hero>(2))
                {
                    h.OnStressed(5, false, false);
                }
                break;

            case 1: // 자해
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("<color=red>그냥 죽는 게 낫지");

                OnDamaged(5, false);
                break;

            case 2: // 턴 넘기기
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("아무것도 하고 싶지 않아.");
                break;

            default:
                break;
        }
    }
}
