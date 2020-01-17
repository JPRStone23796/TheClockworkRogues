using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class SCR_BuffSpider : MonoBehaviour, IEnemy,IDamageable
{
    [Header("Enemy Type")]
    [SerializeField]
    private EnemyType currentEnemyType;
    [Header("Cog Drops")]
    [SerializeField]
    int cogDrops;

    [Space(20)] [Header("Explosion Effect")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionLength = 4.0f;


    private IRoom roomManager;
    [SerializeField]
    private BuffTypes typeOfBuff;

    [SerializeField]
    private float health = 1;

    private SCR_RoomManager roomManagerScript;

    [SerializeField] private GameObject[] generators;
    [SerializeField]
    private bool poweredDown = false, damagable = false;


    //private Renderer rend;

    [SerializeField] private Material damagableMaterial, InvincibleMaterial;

    private SCR_ScannerEffect _scannerEffectScript;

    private bool started = false;
    private SCR_CogPrefabs cogPrefabs;

    private float currentTimer = 0.0f, waveTimer = 5.0f;


    [Space(20)] [Header("Telegraphing")]
    [SerializeField] Renderer pulsingGeneratorsRenderer;
    [SerializeField] private Color invincibleColour, damagableColour;
    [SerializeField] private float pulseSpeed = 0.0f;

    private Color currentEmissionColour;
    private Material pulsingGeneratorsMaterial;

    [Space(20)] [Header("UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject healthUIObject;
    private float maxHealth;

    private AudioSource buffAudioSource;
    [Space(20)][Header("Audio")]
    [SerializeField] private AudioClip buffPowerDownClip;
    [SerializeField] private AudioClip buffReleaseClip;

    void Awake()
    {
        cogPrefabs = Resources.Load<SCR_CogPrefabs>("Cog Prefabs");
        //rend = GetComponent<Renderer>();
        //rend.material = InvincibleMaterial;
        _scannerEffectScript = Camera.main.GetComponent<SCR_ScannerEffect>();
        _scannerEffectScript.enabled = true;
        currentTimer = waveTimer;
        buffAudioSource = GetComponent<AudioSource>();
        pulsingGeneratorsMaterial = pulsingGeneratorsRenderer.material;
        currentEmissionColour = invincibleColour;

        maxHealth = health;
        healthBar.fillAmount = health / maxHealth;
    }

    public EnemyType ReturnEnemyType()
    {
        return currentEnemyType;
    }

    public void SetRoomManager(GameObject rm)
    {
        roomManager = (IRoom)rm.GetComponent(typeof(IRoom));
        roomManagerScript = rm.GetComponent<SCR_RoomManager>();
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

        roomManagerScript.DebuffEnemies();
        _scannerEffectScript.stopWave();
        _scannerEffectScript.enabled = false;
        SpawnCogs();
        Vector3 explosionPos = transform.position;
        explosionPos.y = 0.1f;
        GameObject finalExplosion = Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
        Destroy(finalExplosion, explosionLength);
        Destroy(this.gameObject);
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

    void Update()
    {
        if (!damagable)
        {
            int destroyedEnemies = 0;
            for (int i = 0; i < generators.Length; i++)
            {
                if (generators[i] == null)
                {
                    destroyedEnemies++;
                }
            }

            if (destroyedEnemies == generators.Length)
            {
                damagable = true;
                currentEmissionColour = damagableColour;
                healthUIObject.SetActive(true);
            }
        }


        if (started && !poweredDown)
        {
            currentTimer += Time.deltaTime;

            float currentColourIndex = (Mathf.Sin(Time.time * pulseSpeed) + 1.0f) / 2.0f;
            Color pulseIndicatorColour = Color.Lerp(Color.black, currentEmissionColour, currentColourIndex);
            pulsingGeneratorsMaterial.SetColor("_EmissionColor", pulseIndicatorColour);
            pulsingGeneratorsMaterial.EnableKeyword("_EMISSION");

            if (currentTimer >= waveTimer)
            {
                _scannerEffectScript.BeginWave(transform, roomManagerScript);
                currentTimer = 0.0f;
                buffAudioSource.PlayOneShot(buffReleaseClip);
            }
            
        }
    }

    public void PowerDown()
    {
        for (int i = 0; i < generators.Length; i++)
        {
            if (generators[i])
            {
                IEnemy currentGenerator = (IEnemy)generators[i].GetComponent(typeof(IEnemy));
                currentGenerator.DestroySelf();
            }
         
        }
        poweredDown = true;

        _scannerEffectScript.stopWave();
        _scannerEffectScript.enabled = false;
        //rend.material = InvincibleMaterial;
        buffAudioSource.PlayOneShot(buffPowerDownClip);
    }

    public void DamageEnemy(float bulletDamage)
    {
        UpdateHealth(bulletDamage);
    }

    public void UpdateHealth(float bulletDamage)
    {
        if (damagable && !poweredDown)
        {
            health -= bulletDamage;
            healthBar.fillAmount = health / maxHealth;

            if (health <= 0)
            {
                DestroySelf();
            }
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
        started = true;
    }

    public void BuffEnemy(BuffTypes type)
    {

    }

    public IEnumerator spawnEnemy(GameObject node, float spawnTimer)
    {

        yield return null;
    }


    public BuffTypes returnBuffType()
    {
        return typeOfBuff;
    }


}

	
