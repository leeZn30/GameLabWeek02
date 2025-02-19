using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TechTarget
{
    // 단일, 다중
    Single, Multiple
}

// 사거리 타입
public enum TechRange
{
    // 근거리, 원거리
    Melee, Ranged
}

public enum TechType
{
    // 공격, 스트레스, 힐, 스트레스 힐
    Attack, Stress, Heal, StressHeal
}

public enum Mod
{
    positive, negative
}

[System.Serializable]
public class TechData
{
    public string ID;
    public TechType TechType;
    public TechTarget TechTarget;
    public float Acc; // 명중

    [Header("사거리 관련")]
    public TechRange TechRange;
    public int Range;
    public bool isInternal;

    [Header("데미지(힐) 능력치")]
    public bool isFixedDamage = false;
    // 가변 데미지라면
    public Mod dMGMod;
    public float DamageMod;
    // 고정 데미지라면
    public int FixedMinDamage;
    public int FixedMaxDamage;

    [Header("크리티컬 능력치")]
    public bool isFixedCritical = false;
    public int CriticalMod;
    public int FixedCritical;

    [Header("추가 능력치")]
    public int Stun; // 기절
    public int Bleed; // 출혈
    public int Poison; // 중독
    public bool isStunEnable => Stun > 0;
    public bool isBleedEnable => Bleed > 0;
    public bool isPoisonEnable => Poison > 0;

    [Header("상태 이상 데미지")]
    public int BleedDamage;
    public int BleedTurnCnt;
    public int PoisonDamage;
    public int PoisonTurnCnt;
}
