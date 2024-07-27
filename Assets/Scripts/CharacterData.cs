using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    [Header("기본 능력치")]
    public string ID;
    public int Speed; // 턴 순서를 결정할 속도
    public int MaxHp;
    public int Stress = 0;
    public float AccMod; // 명중 보정치
    public float Dodge; // 회피
    public int Crit; // 크리티컬
    public int Step; // 걸음 수
    public int WillPower = 25; // 각성/붕괴

    [Header("저항력")]
    public int StunResist;
    public int BleedResist;
    public int PoisonResist;
    public int DeathResist;

    public List<TechData> Techs = new List<TechData>
    {
        new TechData() // 캐릭터 별 기본 기술
    };

}
