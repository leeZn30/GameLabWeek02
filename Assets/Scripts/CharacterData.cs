using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string ID;
    public int Speed; // 턴 순서를 결정할 속도
    public int Hp;
    public float Stress;
    public float AccMod; // 명중 보정치
    public float Dodge; // 회피
    public float Crit; // 크리티컬
    public int Step;

    public List<TechData> Techs = new List<TechData>
    {
        new TechData() // 캐릭터 별 기본 기술
    };

}
