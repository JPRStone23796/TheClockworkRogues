using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCR_BossGenerator : MonoBehaviour, IEnemy,IDamageable
{



    [SerializeField]
    private float health = 10;
    private float maxHealth;

 

    [Header("Explosion Effect")]
    [SerializeField]
    GameObject explosionPrefab;
    [SerializeField]
    float explosionLength = 4.0f;

    private SCR_Boss currentBoss;

    void Start()
    {
        maxHealth = health;
        currentBoss = GameObject.FindGameObjectWithTag("BossManager").GetComponent<SCR_Boss>();

    }

    public EnemyType ReturnEnemyType()
    {
        return EnemyType.Buff;
    }

    public void SetRoomManager(GameObject rm)
    {

    }

    public void DestroySelf()
    {
        currentBoss.BossDefeated();
        SCR_GameManager.instance.LevelCompleted();
        Vector3 explosionPos = transform.position;
        explosionPos.y = 0.1f;
        GameObject finalExplosion = Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
        Destroy(finalExplosion, explosionLength);
        Destroy(this.gameObject);
    }

    public void PowerDown()
    {
        Destroy(this.gameObject);
    }


    public void DamageEnemy(float bulletDamage)
    {
        UpdateHealth(bulletDamage);
    }

    public void UpdateHealth(float bulletDamage)
    {
        health -= bulletDamage;

        if (health <= 0)
        {
            DestroySelf();
        }

    }

    public float GetHealth()
    {
        return health;
    }

    public void SetSpeed(float mSpeed)
    {

    }

    public float GetSpeed()
    {
        return 0;
    }

    public void StartAI()
    {
    }

    public void BuffEnemy(BuffTypes type)
    {

    }

    public IEnumerator spawnEnemy(GameObject node, float spawnTimer)
    {

        yield return null;
    }

  

  

}
