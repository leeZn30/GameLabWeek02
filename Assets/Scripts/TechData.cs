using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TechData
{
    public string ID;
    public int Range;
    public bool isInternal;

    [Header("기본 능력치")]
    public float Acc; // 명중
}
