using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("능력치")]
    public CharacterData characterData;
    public int hp => characterData.Hp;
    public int step => characterData.Step;

    [Header("기술")]
    int techIndex = 0;
    public TechData equippedTech => characterData.Techs[techIndex];
    public int attackRange => characterData.Techs[techIndex].Range;
    public bool isAtkRangeInternal => characterData.Techs[techIndex].isInternal;

}
