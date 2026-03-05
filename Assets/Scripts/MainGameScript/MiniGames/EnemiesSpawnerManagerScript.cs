using System.Collections.Generic;
using UnityEngine;

public class EnemiesSpawnerManagerScript : MonoBehaviour
{
    public EnemySurvivalMovementScript enemyPrefab;
    public PlayerMouvementRotationMouse player;
    public int enemiesToSpawn;
    public Transform enemiesManager;
    public float minSpeed;
    public float maxSpeed;
    
    public List<Transform> spawnPoints;
    private int _enemiesInArena;
    private float _nextSpawnTime;

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
        EnemySurvivalMovementScript newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, enemiesManager);
        newEnemy.Player = player.transform;
        newEnemy.Speed = Random.Range(minSpeed, maxSpeed);
        newEnemy.transform.name = enemyPrefab.name;
        
        _enemiesInArena++;
        newEnemy.GetComponent<EnemySurvivalCollisionDetectionScript>().OnKilledByBullet += OnEnemyKilledByBullet;
        newEnemy.GetComponent<EnemySurvivalCollisionDetectionScript>().OnPlayerTouched += OnPlayerTouchedByEnemy;
    }
    
    private void OnEnemyKilledByBullet(EnemySurvivalCollisionDetectionScript enemy)
    {
        enemy.OnKilledByBullet -= OnEnemyKilledByBullet;
        _enemiesInArena--;
        enemiesManager.GetComponent<CharacterHealthScript>().TakeDamage(2);
    }
    
    private void OnPlayerTouchedByEnemy(EnemySurvivalCollisionDetectionScript enemy)
    {
        enemy.OnPlayerTouched -= OnPlayerTouchedByEnemy;
        _enemiesInArena--;
    }
}
