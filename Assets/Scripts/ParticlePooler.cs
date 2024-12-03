using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePooler : MonoBehaviour
{
    [SerializeField]
    private GameObject particlePrefab;

    [SerializeField]
    private int initialPoolSize = 10;

    private Queue<GameObject> particlePool = new Queue<GameObject>();

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject particle = Instantiate(particlePrefab);
            particle.SetActive(false);
            particlePool.Enqueue(particle);
        }
    }

    public GameObject GetParticle()
    {
        if (particlePool.Count > 0)
        {
            GameObject particle = particlePool.Dequeue();
            particle.SetActive(true);
            return particle;
        }
        else
        {
            // 풀에 남은 오브젝트가 없으면 새로 생성
            GameObject particle = Instantiate(particlePrefab);
            return particle;
        }
    }

    public void ReturnParticle(GameObject particle)
    {
        particle.SetActive(false);
        particlePool.Enqueue(particle);
    }
}