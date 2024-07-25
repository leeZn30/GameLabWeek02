using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroData
{
    public string ID;
    public int Hp;
    public float Stress;
    public float AccMod; // 명중 보정치
    public float Dodge; // 회피
    public float Crit; // 크리티컬
    public int Step;
}
