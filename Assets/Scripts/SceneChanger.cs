using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : SingleTon<SceneChanger>
{
    [SerializeField] Button ruin;
    [SerializeField] Button forest;
    [SerializeField] Button coast;
    [SerializeField] Button quit;

    void Awake()
    {
        ruin.onClick.AddListener(() => LoadScene("RuinsScene"));
        forest.onClick.AddListener(() => LoadScene("ForestScene"));
        coast.onClick.AddListener(() => LoadScene("CoastScene"));

        quit.onClick.AddListener(() => Application.Quit());
    }

    void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
