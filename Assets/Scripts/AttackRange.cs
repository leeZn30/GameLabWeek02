using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    public EnemyAI enemy;
    public Hero hero;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemy = other.GetComponent<EnemyAI>();
        }
        else if (other.CompareTag("Player"))
        {
            hero = other.GetComponent<Hero>();
        }
    }
}
