using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerScript : MonoBehaviour
{
    public List<GameObject> enemyPrefabs;
    public Transform[] spawnPoints;
    public float spawnInterval = 3f;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnEnemy", spawnInterval, spawnInterval);
    }

    void SpawnEnemy()
    {
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnIndex];

        int randomEnemyPrefabIndex = Random.Range(0, enemyPrefabs.Count);
        Instantiate(enemyPrefabs[randomEnemyPrefabIndex], spawnPoint.position, spawnPoint.rotation);
    }
}
