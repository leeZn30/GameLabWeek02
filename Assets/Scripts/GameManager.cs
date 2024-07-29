
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingleTon<GameManager>
{
    public bool isGameProgressing;
    List<Hero> aliveHeroes = new List<Hero>();
    List<EnemyAI> aliveEnemies = new List<EnemyAI>();
    public GameObject Characters;

    void Awake()
    {
        SetPlayerLocation();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isGameProgressing)
        {
            if (UIManager.Instance.GetReadyCharacter() == 6)
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
            if (go.transform.parent != UIManager.Instance.locateUI.transform)
                Instantiate(UnitData.Instance.characters.Find(e => e.characterData.ID == go.ID), go.transform.position, Quaternion.identity, Characters.transform);
            Destroy(go.gameObject);
        }

        UIManager.Instance.HideStateUI(new Hero());
        UIManager.Instance.locateUI.SetActive(false);
        UIManager.Instance.HideGameInfo();

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
        UIManager.Instance.menuText.SetText("원정 성공");

        isGameProgressing = false;

        UIManager.Instance.OpenMenu();
    }

    public void FailGame()
    {
        UIManager.Instance.menuText.SetText("원정 실패");

        isGameProgressing = false;

        UIManager.Instance.OpenMenu();
    }

    public void ReLoadScene()
    {
        string str = SceneManager.GetActiveScene().name;
        Debug.Log(str);
        SceneManager.LoadScene(str);
    }

    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
}
