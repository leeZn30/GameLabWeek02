using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
struct CharacterUI
{
    public string name;
    public GameObject go;
}

public class UnitData : SingleTon<UnitData>
{
    public List<Hero> characters = new List<Hero>();
    [SerializeField] List<CharacterUI> characterUIs = new List<CharacterUI>();

    public List<string> unitNames = new List<string>();

    public GameObject GetCharacterOfUI(string name)
    {
        return characterUIs.Find(e => e.name == name).go;
    }
}
