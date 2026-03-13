using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemiesSpawnerManagerScript : MonoBehaviour
{
    public EnemySurvivalMovementScript enemyPrefab;
    public PlayerMouvementRotationMouse player;
    public int enemiesToSpawn = 6;
    private int _enemiesToSpawnDefault = 6;
    public Transform enemiesManager;
    public float minSpeed;
    public float maxSpeed;
    
    public List<Transform> spawnPoints;
    private int _enemiesInArena;

    private int _enemiesDamagedByBulletsDefault = 2;
    private int _enemiesDamagedByBullets = 2;

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
        _enemiesInArena++;
        
        int spawnIndex = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[spawnIndex];
        EnemySurvivalMovementScript newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation, enemiesManager);
        newEnemy.Player = player.transform;
        newEnemy.Speed = Random.Range(minSpeed, maxSpeed);
        newEnemy.transform.name = enemyPrefab.name;
        
        newEnemy.GetComponent<EnemySurvivalCollisionDetectionScript>().OnKilledByBullet += OnEnemyKilledByBullet;
        newEnemy.GetComponent<EnemySurvivalCollisionDetectionScript>().OnPlayerTouched += OnPlayerTouchedByEnemy;
    }
    
    private void OnEnemyKilledByBullet(EnemySurvivalCollisionDetectionScript enemy)
    {
        enemy.OnKilledByBullet -= OnEnemyKilledByBullet;
        _enemiesInArena--;
        enemiesManager.GetComponent<CharacterHealthScript>().TakeDamage(_enemiesDamagedByBullets);
    }
    
    private void OnPlayerTouchedByEnemy(EnemySurvivalCollisionDetectionScript enemy)
    {
        enemy.OnPlayerTouched -= OnPlayerTouchedByEnemy;
        _enemiesInArena--;
    }

    void OnDisable()
    {
        Reset();
    }

    private void Reset()
    {
        enemiesToSpawn = _enemiesToSpawnDefault;
        _enemiesDamagedByBullets = _enemiesDamagedByBulletsDefault;
        
        DestroyAllEnemies();
    }

    private void DestroyAllEnemies()
    {
        _enemiesInArena = 0;
        foreach (Transform child in enemiesManager) 
        {
            Destroy(child.gameObject);
        }
    }

    public void PlayerLost()
    {
        enemiesToSpawn = 25;
        _enemiesDamagedByBullets = 0;
    }

    public void PlayerWon()
    {
        DestroyAllEnemies();
    }
}
