using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StateUI : MonoBehaviour
{
    RectTransform rect;
    RectTransform canvasRectTransform;

    [Header("캐릭터 능력치")]
    [SerializeField] TextMeshProUGUI characterNameText;
    [SerializeField] TextMeshProUGUI stepText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI defenseText;
    [SerializeField] TextMeshProUGUI dodgeText;
    [SerializeField] TextMeshProUGUI stunResistText;
    [SerializeField] TextMeshProUGUI bleedResistText;
    [SerializeField] TextMeshProUGUI poisonResistText;

    [Header("기술 능력치")]
    [SerializeField] TextMeshProUGUI skillIndexText;
    [SerializeField] TextMeshProUGUI skillNameText;
    [SerializeField] TextMeshProUGUI skillTypeText;
    [SerializeField] TextMeshProUGUI skillTargetText;
    [SerializeField] TextMeshProUGUI stunText;
    [SerializeField] TextMeshProUGUI bleedText;
    [SerializeField] TextMeshProUGUI bleedDescText;
    [SerializeField] TextMeshProUGUI poisonText;
    [SerializeField] TextMeshProUGUI poisonDescText;


    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasRectTransform = transform.parent.GetComponent<RectTransform>();
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowStat(Character character)
    {
        CharacterData data = character.characterData;

        characterNameText.SetText(data.ID);
        stepText.SetText(data.Step.ToString());
        speedText.SetText(data.Speed.ToString());
        defenseText.SetText(data.defense.ToString());
        dodgeText.SetText(data.Dodge.ToString());
        stunResistText.SetText(data.StunResist.ToString());
        bleedResistText.SetText(data.BleedResist.ToString());
        poisonResistText.SetText(data.PoisonResist.ToString());

        skillIndexText.SetText((character.techIndex + 1).ToString());
        skillNameText.SetText(character.equippedTech.ID);
        switch (character.equippedTech.TechType)
        {
            case TechType.Attack:
                skillTypeText.SetText("물리 공격");
                break;

            case TechType.Stress:
                skillTypeText.SetText("스트레스 공격");
                break;

            case TechType.Heal:
                skillTypeText.SetText("체력 힐");
                break;

            case TechType.StressHeal:
                skillTypeText.SetText("스트레스 힐");
                break;
        }
        switch (character.equippedTech.TechTarget)
        {
            case TechTarget.Single:
                skillTargetText.SetText("단일");
                break;

            case TechTarget.Multiple:
                skillTargetText.SetText("다중");
                break;
        }
        stunText.SetText(character.equippedTech.Stun.ToString());
        bleedText.SetText(character.equippedTech.Bleed.ToString());
        bleedDescText.SetText(string.Format("{0} 턴당 {1} 데미지", character.equippedTech.BleedTurnCnt, character.equippedTech.BleedDamage));
        poisonText.SetText(character.equippedTech.Poison.ToString());
        poisonDescText.SetText(string.Format("{0} 턴당 {1} 데미지", character.equippedTech.PoisonTurnCnt, character.equippedTech.PoisonDamage));

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // transform.position = character.transform.position + new Vector3(0, 5, 0);
        locate(character.transform);
    }

    public void hideStat()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    void locate(Transform tf)
    {
        transform.position = tf.position + new Vector3(0, 5);

        // float halfWidth = rect.rect.width / 2;
        // float halfHeight = rect.rect.height / 2;

        // Vector2 Right = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height * 0.5f));
        // Vector2 Left = -Right;
        // Vector2 Top = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width * 0.5f, Screen.height));
        // // Vector2 Bottom = -Top;

        // // 위에서 잘리는지 확인
        // if (Camera.main.WorldToViewportPoint(transform.position + new Vector3(0, halfHeight, 0)).y > 1f)
        //     transform.position = new Vector2(transform.position.x, tf.position.y - 4);

        // if (transform.position.y + halfHeight > Top.y)
        //     transform.position = new Vector2(transform.position.x, tf.position.y - 1);

        // // 좌에서 잘리는지 확인
        // if (transform.position.x - halfWidth < Left.x)
        //     transform.position = new Vector2(transform.position.x + halfWidth, transform.position.y);

        // // 우에서 잘리는지 확인
        // if (transform.position.x + halfWidth > Right.x)
        //     transform.position = new Vector2(transform.position.x - halfWidth, transform.position.y);

        // Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        // if (pos.x < 0f) pos.x = 0f;
        // if (pos.x > 1f) pos.x = 1f;
        // if (pos.y < 0f) pos.y = 0f;
        // if (pos.y > 1f) pos.y = 1f;
        // transform.position = Camera.main.ViewportToWorldPoint(pos);
    }
}
