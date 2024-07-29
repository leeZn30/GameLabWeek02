
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : SingleTon<GameManager>
{
    bool isGameProgressing;
    List<Hero> aliveHeroes;
    List<EnemyAI> aliveEnemies;

    void Awake()
    {
        SetPlayerLocation();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isGameProgressing)
        {
            if (UIManager.Instance.GetReadyCharacter() == 0)
            {
                UIManager.Instance.CloseLocateUI();
                StartGame();
            }
        }

        ObserveGame();
    }

    void SetPlayerLocation()
    {
        UIManager.Instance.SetLocateUI();

        foreach (Vector3Int pose in GridHighlighter.Instance.InitialPlayerPositions)
        {
            GridHighlighter.Instance.HighlightSpecificTile(pose);
        }
    }

    void StartGame()
    {
        isGameProgressing = true;

        DragDropHandler[] gos = FindObjectsOfType<DragDropHandler>();

        foreach (DragDropHandler go in gos)
        {
            Instantiate(UnitData.Instance.characters.Find(e => e.characterData.ID == go.ID), go.transform.position, Quaternion.identity);
            Destroy(go.gameObject);
        }

        aliveHeroes = FindObjectsOfType<Hero>().ToList();
        aliveEnemies = FindObjectsOfType<EnemyAI>().ToList();

        GridHighlighter.Instance.UnHighlightAllTile();

        TurnManager.Instance.StartInitTurn();
    }

    public void RemoveCharacter(Character c)
    {
        if (c is Hero)
        {
            aliveHeroes.Remove(c as Hero);
        }
        else
        {
            aliveEnemies.Remove(c as EnemyAI);
        }
    }

    void ObserveGame()
    {
        if (isGameProgressing)
        {
            if (aliveHeroes.Count == 0)
                FailGame();

            if (aliveEnemies.Count == 0)
                ClearGame();
        }
    }

    public void ClearGame()
    {
        Debug.Log("Clear");
    }

    public void FailGame()
    {
        Debug.Log("Fail");
    }
}
