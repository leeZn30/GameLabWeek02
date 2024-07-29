using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class CharacterComparer : IComparer<Character>
{
    public int Compare(Character x, Character y)
    {
        if (x == null || y == null) return 0;

        // 내림차순으로 정렬
        int result = y.nowSpeed.CompareTo(x.nowSpeed);
        if (result == 0)
        {
            // 초기 speed값이 높은 캐릭터가 우선
            result = y.characterData.Speed.CompareTo(x.characterData.Speed); // 이름으로 비교 (초기 speed값 저장 방법이 없으므로 임시로 이름으로 비교)
        }
        return result;
    }
}

public class TurnManager : SingleTon<TurnManager>
{
    public Character nowTurnCharacter;

    SortedSet<Character> priorityQueue;

    Coroutine turn;

    void Start()
    {
    }

    public void StartInitTurn()
    {
        StartCoroutine(orderCharacter());
    }

    IEnumerator orderCharacter()
    {
        UIManager.Instance.ShowGameInfo("전투 순서 정비 중...");

        yield return new WaitForSeconds(2f);

        UIManager.Instance.HideGameInfo();

        // 1. 화면에 있는 모든 Character 타입의 오브젝트를 찾는다.
        Character[] characters = FindObjectsOfType<Character>();

        // 2. 각 캐릭터의 speed 변수에 1~8 사이의 랜덤값을 더한다.
        System.Random random = new System.Random();
        foreach (Character character in characters)
        {
            int randomValue = random.Next(1, 9);
            character.nowSpeed = character.characterData.Speed + randomValue;
        }

        // 3. 우선순위 큐에 내림차순으로 정렬한다. 값이 같다면 초기 speed가 높은 캐릭터가 우선.
        priorityQueue = new SortedSet<Character>(new CharacterComparer());

        foreach (Character character in characters)
        {
            priorityQueue.Add(character);
            character.createTurnUI();
        }

        // 첫번째 시작
        nowTurnCharacter = priorityQueue.First();

        yield return StartCoroutine(pointNowTurnCharacter());

        nowTurnCharacter.removeTurnUI();
        priorityQueue.First().StartTurn();
        priorityQueue.Remove(priorityQueue.First());
    }

    public void StartNextTurn()
    {
        UIManager.Instance.HideAccDmgInfo();
        UIManager.Instance.HideHealInfo();
        StartCoroutine(waitTurn());
    }

    IEnumerator waitTurn()
    {
        if (turn != null)
        {
            yield return turn;
            turn = StartCoroutine(passTurn());
        }
        else
            turn = StartCoroutine(passTurn());
    }

    IEnumerator passTurn()
    {
        nowTurnCharacter.myTurn = false;

        if (priorityQueue.Count == 0)
        {
            StartCoroutine(orderCharacter());
            yield break;
        }

        // 죽을 때 알아서 지우게 함
        nowTurnCharacter = priorityQueue.First();

        yield return StartCoroutine(pointNowTurnCharacter());

        nowTurnCharacter.removeTurnUI();
        nowTurnCharacter.StartTurn();
        priorityQueue.Remove(nowTurnCharacter);
    }

    IEnumerator pointNowTurnCharacter()
    {
        nowTurnCharacter.Light.gameObject.SetActive(true);
        for (float t = 0; t < 1.5; t += Time.deltaTime)
        {
            float alpha = Mathf.PingPong(t * 10f, 1f);
            nowTurnCharacter.Light.color = new Color(nowTurnCharacter.Light.color.r, nowTurnCharacter.Light.color.g, nowTurnCharacter.Light.color.b, alpha);
            yield return null;
        }
        nowTurnCharacter.Light.gameObject.SetActive(false);
    }

    public void removeCharacterFromQueue(Character character)
    {
        if (priorityQueue != null && priorityQueue.Count > 0)
        {
            priorityQueue.Remove(character);
        }
    }
}
