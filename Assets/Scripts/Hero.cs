using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Hero : Character
{
    [Header("상태")]
    public Vector3Int CurrentTilePosition;
    bool isChoosing;
    int choosingType;
    public bool isDeathDoor;

    [Header("UI")]
    HeroUI HeroUI;

    void Start()
    {
        // 대충 놔도 스냅되도록
        tilemap = GridHighlighter.Instance.tilemap;
        transform.position = GridHighlighter.Instance.ConvertTileToWorldPosition(tilemap.WorldToCell(transform.position));

        HeroUI = Instantiate(CharacterUIPfb, transform.position + CharacterUIPositionOffset, Quaternion.identity, GameObject.Find("CharacterUIs").transform).GetComponent<HeroUI>();
        HeroUI.Init(this);
    }

    protected override void Update()
    {
        base.Update();

        HeroUI.transform.position = transform.position + CharacterUIPositionOffset;

        // 전투 기술 바꾸기
        ReEquipSkill();

        // 단일 타겟 기술 캐릭터 고르기
        if (isChoosing && Input.GetMouseButton(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.down, 1f, 1 << LayerMask.NameToLayer("Character"));

            if (choosingType == 0)
            {
                if (hit.collider != null && hit.collider.CompareTag("Enemy"))
                {
                    CombatManager.Instance.Combat(this, hit.collider.GetComponent<EnemyAI>());
                    GridHighlighter.Instance.RemoveAllAttackRange();

                    isChoosing = false;

                    UIManager.Instance.HideGameInfo();
                }
            }
            else
            {
                if (hit.collider != null && hit.collider.CompareTag("Player") && hit.collider.gameObject != gameObject)
                {
                    // 자힐
                    bool crit = CombatManager.Instance.isCritical(this);
                    int heal = CombatManager.Instance.GetHeal(this, crit);
                    if (equippedTech.TechType == TechType.Heal)
                    {
                        OnHealed(heal, crit);
                    }
                    else
                    {
                        OnStressHealed(heal, crit);
                    }

                    CombatManager.Instance.Combat(this, hit.collider.GetComponent<Hero>());
                    GridHighlighter.Instance.RemoveAllAttackRange();

                    isChoosing = false;

                    UIManager.Instance.HideGameInfo();
                }
            }
        }
    }

    void OnMouseOver()
    {
        if (myTurn && (equippedTech.TechType == TechType.Heal || equippedTech.TechType == TechType.StressHeal))
        {
            UIManager.Instance.ShowHealInfo(this);
        }
        else
        {
            UIManager.Instance.HideHealInfo();
        }
    }
    void OnMouseExit()
    {
        if (myTurn)
        {
            UIManager.Instance.HideHealInfo();
        }
    }


    protected override void OperateCharacter()
    {
        if (myTurn)
        {
            UIManager.Instance.ShowStateUI(this);

            CurrentTilePosition = GridHighlighter.Instance.GetCurrentTilePosition(transform);

            GridHighlighter.Instance.selectedHero = this;
            GridHighlighter.Instance.HighlightStartTile(CurrentTilePosition);
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

            // 이동만 한 것
            if (targets.Count == 0)
            {
                GridHighlighter.Instance.RemoveAllAttackRange();

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
                        ReadyToChooseCharacter(0);
                    }
                    // 다중 공격이라면 모두에게 적용
                    else
                    {
                        CombatManager.Instance.Combat(this, targets);
                        GridHighlighter.Instance.RemoveAllAttackRange();
                    }
                }
                // 적 하나 발견
                else
                {
                    if (targets.Count > 0)
                    {
                        CombatManager.Instance.Combat(this, targets[0]);
                        GridHighlighter.Instance.RemoveAllAttackRange();
                    }
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

            // 이동만 한 것
            if (targets.Count == 0)
            {
                GridHighlighter.Instance.RemoveAllAttackRange();

                // 턴 넘길 준비
                GoToNextTurn();
            }
            else
            {
                // 플레이어 하나 이상 발견
                if (targets.Count > 1)
                {
                    // 단일 힐이라면 하나 선택
                    if (equippedTech.TechTarget == TechTarget.Single)
                    {
                        ReadyToChooseCharacter(1);
                    }
                    // 다중 힐이라면 모두에게 적용
                    else
                    {
                        // 자힐
                        bool crit = CombatManager.Instance.isCritical(this);
                        int heal = CombatManager.Instance.GetHeal(this, crit);
                        if (equippedTech.TechType == TechType.Heal)
                        {
                            OnHealed(heal, crit);
                        }
                        else
                        {
                            OnStressHealed(heal, crit);
                        }

                        CombatManager.Instance.Combat(this, targets);
                        GridHighlighter.Instance.RemoveAllAttackRange();
                    }
                }
                // 플레이어 하나 발견
                else
                {
                    if (targets.Count > 0)
                    {
                        // 자힐
                        bool crit = CombatManager.Instance.isCritical(this);
                        int heal = CombatManager.Instance.GetHeal(this, crit);
                        if (equippedTech.TechType == TechType.Heal)
                        {
                            OnHealed(heal, crit);
                        }
                        else
                        {
                            OnStressHealed(heal, crit);
                        }

                        CombatManager.Instance.Combat(this, targets[0]);
                        GridHighlighter.Instance.RemoveAllAttackRange();
                    }
                }

            }
        }
    }

    public override void OnDamaged(int damage, bool isCritical, bool isEffect = true, bool isStatusAbnormal = false)
    {
        base.OnDamaged(damage, isCritical);

        // Hero에서 필요
        if (hp <= 0)
        {
            if (!isDeathDoor)
            {
                // 죽음의 문턱
                hp = 0;
                isDeathDoor = true;
                UIManager.Instance.ShowGameInfo(string.Format("{0}이 죽음의 문턱에 섰습니다.\n회복 없이 이후 공격 받은면, 바로 죽을 수도 있습니다.", characterData.ID));
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

                    if (isStatusAbnormal)
                        isSkipTurn = true;
                    else
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
            desc.SetText(string.Format("<color=#702B76>치명타!\n{0}", stress));
        else
            desc.SetText(string.Format("<color=#702B76>{0}", stress));

        if (isEffect)
            Instantiate(sts, transform.position, sts.transform.rotation);
        // StartCoroutine(stressed());

        this.stress += stress;

        ChangeStressState();
    }

    public override void OnHealed(int healMount, bool isCritical)
    {
        TextMeshProUGUI desc = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
        if (isCritical)
            desc.SetText(string.Format("<color=#9BFF00>치명타! {0}", healMount));
        else
            desc.SetText(string.Format("<color=#9BFF00>{0}", healMount));

        // StartCoroutine(healed());
        Instantiate(heal, transform.position, heal.transform.rotation);

        hp += healMount;

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
        // StartCoroutine(stresshealed());
        Instantiate(stressheal, transform.position, stressheal.transform.rotation);

        stress -= heal;
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
        StartCoroutine(StressChange());
    }

    IEnumerator StressChange()
    {
        // 각성/붕괴 결정
        if (stress >= 50 && StressState == 0)
        {
            string text = string.Format("{0}의 의지가 시험받고 있습니다...", characterData.ID);
            UIManager.Instance.ShowGameInfo(text);

            yield return new WaitForSeconds(2f);

            if (Random.Range(0, 101) < characterData.WillPower)
            {
                // 각성
                StressState = 1;
                text = "각성!";
                UIManager.Instance.ShowGameInfo(text);

                yield return new WaitForSeconds(2f);

                UIManager.Instance.HideGameInfo();
            }
            else
            {
                // 붕괴
                StressState = 2;
                text = "붕괴!";
                UIManager.Instance.ShowGameInfo(text);

                yield return new WaitForSeconds(2f);

                UIManager.Instance.HideGameInfo();
            }
        }
        // 사망
        else if (stress >= 100)
        {
            string text = string.Format("{0} 심장마비", characterData.ID);
            // UIManager.Instance.ShowGameInfo(text);
            TextMeshProUGUI txt = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
            txt.SetText(text);

            Destroy(gameObject);
        }
    }

    void ReadyToChooseCharacter(int characterType)
    {
        if (!isChoosing)
        {
            choosingType = characterType;
            isChoosing = true;
        }

        // UI 뜨게
        string text = "단일 타겟 기술입니다.\n원하는 캐릭터를 클릭하세요.";
        UIManager.Instance.ShowGameInfo(text);
    }

    public void MoveHero()
    {
        CurrentTilePosition = GridHighlighter.Instance.GetCurrentTilePosition(transform);
        GridHighlighter.Instance.selectedHero = null;
        GridHighlighter.Instance.UnHighlightAllTile();
        Attack();
    }


    // Hero에서 구현
    public override void DoAwakening()
    {
        int index = Random.Range(0, 2);
        TextMeshProUGUI text = null;

        switch (index)
        {
            case 0: // 스트레스 회복 전파
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("<color=white>아직 모두 할 수 있어.\n(스트레스 회복 및 전파)");

                OnStressHealed(10, false);

                foreach (Hero h in GetNearHeroes<Hero>(2))
                {
                    h.OnStressHealed(5, false);
                }
                break;

            case 1: // 자힐
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("<color=#9BFF00>저에게 힘을 주소서.\n(체력 회복)");

                OnHealed(5, false);
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
                text.SetText("<color=702B76>이 판은 망했어.\n(스트레스 증가 및 전파)");

                OnStressed(10, false, false);

                foreach (Hero h in GetNearHeroes<Hero>(2))
                {
                    h.OnStressed(5, false, false);
                }
                break;

            case 1: // 자해
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("<color=red>그냥 죽는 게 낫지.\n(자해)");

                OnDamaged(5, false);
                break;

            case 2: // 턴 넘기기
                text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("아무것도 하고 싶지 않아.\n(턴 넘기기)");
                isSkipTurn = true;
                break;

            default:
                break;
        }
    }

    void ReEquipSkill()
    {
        if (myTurn && !isChoosing)
        {
            UIManager.Instance.ShowStateUI(this);

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TextMeshProUGUI text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("장착 기술 변경 1");

                techIndex = 0;

                GridHighlighter.Instance.showAttackRange
                (
                    GridHighlighter.Instance.NowHighlightedPosition,
                    equippedTech.Range,
                    equippedTech.isInternal
                );

            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TextMeshProUGUI text = Instantiate(DescUIPfb, DescGrid.transform).GetComponent<TextMeshProUGUI>();
                text.SetText("장착 기술 변경 2");
                techIndex = 1;

                GridHighlighter.Instance.showAttackRange
                (
                    GridHighlighter.Instance.NowHighlightedPosition,
                    equippedTech.Range,
                    equippedTech.isInternal
                );

            }
        }
    }

    public override void removeTurnUI()
    {
        HeroUI.removeTurnUI();
    }

    public override void createTurnUI()
    {
        HeroUI.createTurnUI();
    }
}
