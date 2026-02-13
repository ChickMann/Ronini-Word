using System;
using ControlManager;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyProfile", menuName = "Scriptable Objects/EnemyProfile")]
public class EnemyProfile : ScriptableObject
{
    public String enemyName;
    public float FuelGauge;
    public EnemyController prefabEnemy;
    public float spawnDistanceX;
    public EnemyProfile()
    {
        spawnDistanceX = 16f;
        FuelGauge = 5f;
    }
    public EnemyController getPrefabEnemy()
    {
        if (prefabEnemy) return prefabEnemy;
        return null;
    }
    
}
