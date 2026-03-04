using System.Collections.Generic;
using UnityEngine;

public class EnemiesSpawnerManagerScript : MonoBehaviour
{
    public EnemySurvivalMouvementScript enemyPrefab;
    public PlayerMouvementRotationMouse player;
    public int enemiesToSpawn;
    public float spawnInterval;
    public Transform gameplayArena;
    public float minSpeed;
    public float maxSpeed;
    
    public List<Transform> spawnPoints;
    private int _enemiesInArena;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_enemiesInArena < enemiesToSpawn)
        {
            SpawnEnemy();
        }
    }
    
    private void SpawnEnemy()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[spawnIndex];
        EnemySurvivalMouvementScript newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, gameplayArena);
        newEnemy.Player = player.transform;
        newEnemy.Speed = Random.Range(minSpeed, maxSpeed);
        _enemiesInArena++;
    }
}
