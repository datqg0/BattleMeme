using UnityEngine;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [Header("List Enemy Random")]
    public List<UnitData> enemyPool; 

    public float spawnInterval = 5f;
    private float nextSpawnTime;

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnRandomEnemy()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentLevel == null) return;

        List<UnitData> levelEnemies = GameManager.Instance.currentLevel.levelEnemies;

        if (levelEnemies == null || levelEnemies.Count == 0) 
        {
            Debug.LogWarning("Chưa có danh sách quái cho màn này!");
            return;
        }

        // Chọn ngẫu nhiên một con lính trong danh sách của màn này
        int randomIndex = Random.Range(0, levelEnemies.Count);
        UnitData selectedEnemy = levelEnemies[randomIndex];

        // Gọi GameManager để sinh ra con đó
        GameManager.Instance.SpawnEnemyUnit(selectedEnemy);
    }
}
