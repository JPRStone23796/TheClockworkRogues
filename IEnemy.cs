using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//struct that stores each possible type of enemy within the game

public enum EnemyType
{
    Grunt,
    Ticker,
    Sonar,
    Turret,
    Buff
};



public enum AttackType
{
    Closest,HighestHealth, MostPowerful,Player1,Player2
};


public interface IEnemy
{


 

    EnemyType ReturnEnemyType();

    void SetRoomManager(GameObject rm);

    void DestroySelf();

    void UpdateHealth(float bulletDamage);

    float GetHealth();

    void SetSpeed(float mSpeed);

    float GetSpeed();

    void StartAI();

    void BuffEnemy(BuffTypes type);

 

    IEnumerator spawnEnemy(GameObject node, float spawnTimer);
}

