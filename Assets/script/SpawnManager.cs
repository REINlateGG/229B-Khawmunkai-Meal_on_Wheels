using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public Transform[] spawnPoints; // จุดเกิดของอุปสรรค
    public float spawnRate = 1.5f; // ความถี่ในการเกิดอุปสรรค
    public int minObstaclesPerSpawn = 1;
    public int maxObstaclesPerSpawn = 3; // จำนวนอุปสรรคสุ่มเกิดต่อรอบ

    void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), 1f, spawnRate);
    }

    void SpawnObstacle()
    {
        int numObstacles = Random.Range(minObstaclesPerSpawn, maxObstaclesPerSpawn + 1); // สุ่มจำนวนที่เกิด

        for (int i = 0; i < numObstacles; i++)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Length);
            Instantiate(obstaclePrefab, spawnPoints[spawnIndex].position, Quaternion.identity);
        }
    }
}
