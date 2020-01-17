using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCR_Generator : MonoBehaviour,IEnemy,IDamageable
{



    [SerializeField] private float health = 1;
    private float maxHealth;

    [Header("Cog Drops")]
    [SerializeField] int cogDrops;                    

    private SCR_CogPrefabs cogPrefabs;

    [Header("Explosion Effect")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionLength = 4.0f;

    
    [Header("UI")]
    [SerializeField] private Image healthBar;

    [SerializeField] private GameObject healthUIObject;

    void Start()
    {
        cogPrefabs = Resources.Load<SCR_CogPrefabs>("Cog Prefabs");
        maxHealth = health;
        healthBar.fillAmount = health / maxHealth;
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
        StartCoroutine(DestructionProcess());
    }


    IEnumerator DestructionProcess()
    {
        float t = 0.0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            float currentScale = Mathf.Lerp(1, 2, t / 0.12f);
            healthUIObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }
        t = 0.0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float currentScale = Mathf.Lerp(2, 0, t / 0.2f);
            healthUIObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }

        SpawnCogs();
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
        healthBar.fillAmount = health / maxHealth;
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

   


    void SpawnCogs()
    {
        Vector3 inititalDirection = Vector3.up;
        inititalDirection.x = Random.Range(0, 1.0f);
        inititalDirection.z = Random.Range(0, 1.0f);
        float angle = 360.0f / (float)cogDrops;
        float startAngle = 0;

        for (int i = 0; i < cogDrops; i++)
        {
            int rng = Random.Range(0, cogPrefabs.CogPrefabsList.Count);
            GameObject selectedCog = cogPrefabs.CogPrefabsList[rng];
            Vector3 spawnPos = transform.position;
            spawnPos.y += selectedCog.transform.lossyScale.y;
            GameObject cogModel = Instantiate(selectedCog, spawnPos, Quaternion.identity);
            GameObject cogObj = Instantiate(cogPrefabs.cogObj, spawnPos, Quaternion.identity);
            cogModel.transform.parent = cogObj.transform;



            float useAngle = startAngle + Random.Range(-15.0f, 15.0f);
            Vector3 cogDir = Quaternion.AngleAxis(useAngle, Vector3.up) * inititalDirection;
            cogObj.GetComponent<SCR_CogMovement>().SetDirection(cogDir);
            startAngle += angle;
        }
    }

}
