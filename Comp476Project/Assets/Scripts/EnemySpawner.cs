﻿using UnityEngine;
using UnityEngine.Networking;

public class EnemySpawner : NetworkBehaviour
{
    public GameObject EnemyPrefab;
    public int NumberOfEnemies;

    public override void OnStartServer()
    {
        for (int i = 0; i < NumberOfEnemies; i++)
        {
            var spawnPosition = new Vector3(Random.Range(-8.0f, 8.0f), 2.0f, Random.Range(-8.0f, 8.0f));
            var spawnRotation = Quaternion.Euler(0.0f, Random.Range(0, 180), 0.0f);

            var enemy = Instantiate(EnemyPrefab, spawnPosition, spawnRotation);
            NetworkServer.Spawn(enemy);
        }
    }
}