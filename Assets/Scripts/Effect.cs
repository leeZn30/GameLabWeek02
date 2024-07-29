using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    ParticleSystem particle;

    void Awake()
    {
    }

    void Start()
    {
        StartCoroutine(dissapear());
    }

    IEnumerator dissapear()
    {
        yield return new WaitForSeconds(2f);

        Destroy(gameObject);
    }

}
