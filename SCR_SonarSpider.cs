using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum sonarBehaviourModes
{
    chasing,
    charging,
    tickerSpawn, 
    firing
}




public class SCR_SonarSpider : MonoBehaviour,IEnemy,IDamageable
{

    [Header("Enemy Type")]
    [SerializeField]
    private EnemyType currentEnemyType;

    [Header("Target Selection")]
    [SerializeField]
    private AttackType enemyTargetSelectionType;
    [SerializeField] private LayerMask hitDetectionLayerMask;


    private GameObject player1, player2, currentTarget;

    private IRoom roomManager;
    private GameObject roomManagerObject;

    [Header("Sonar Health")]
    [SerializeField]
    private float health = 100;
    private float maxHealth;

    private NavMeshAgent sonarAgent;


    [Header("AI Movement Speed")]
    [Range(1, 5)]
    [SerializeField]
    private float movementSpeed;
    public float setMovementSpeed
    {
        get { return movementSpeed; }
        set { movementSpeed = value; }
    }

    [Header("AI Rotation Speed")]
    [Range(0.2f, 5)]
    [SerializeField]
    private float rotationSpeed;


    [Header("Tank Gameobjects")]
    [SerializeField]
    private GameObject turret, shield, weakZone;

    [Header("Position Values")]
    [SerializeField]
    private Transform turretEndPosition;
    

    [Header("Behaviour Timers")]
    [SerializeField]
    [Range(1, 15)]
    private float chaseTimer = 6.0f, tickerSpawnTimer = 3.0f, tickerSpawnOffsetDelay = 2.0f, chargeUpTimer = 4.074f , shieldInTimer = 0.5f, firingTimer = 4.0f;

    [Header("Cog Drops")]
    [SerializeField]
    int cogDrops;

    [Header("Explosion Effect")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionLength = 4.0f;

    private float timer;

    [SerializeField]
    private sonarBehaviourModes currentMode;



    [SerializeField]
    private LineRenderer projectileBeam;

    private float projectileRange = 1000f;


    [SerializeField]
    private GameObject tickerPrefab;

    [SerializeField]
    private Transform tickerSpawn,tickerExit;


    private bool started = false;


    [SerializeField]
    private int numberOfTickersSpawned = 3;
    private SCR_CogPrefabs cogPrefabs;

    [Header("UI")]
    [SerializeField] private Image  healthBar;
    [SerializeField] private GameObject healthUIObject;
    [SerializeField] private GameObject buffIcon;

    private Animator sonarAnim;
    private bool bTickerSpawned;

    [Header("Shooting Particles")]
    [SerializeField] private GameObject sonarMuzzle;
    [SerializeField] private GameObject sonarBullet;
    [SerializeField] private GameObject sonarHit;

    [Header("Stun Effect")]
    [SerializeField] [Range(0.1f, 10.0f)] private float stunTime = 2.0f;
    [SerializeField] [Range(0.0f, 10.0f)] private float stunSpeed = 2.0f;
    [SerializeField] [Range(0.5f, 10.0f)] private float damageRadius = 2.0f;
    [SerializeField] [Range(1.0f, 50.0f)] private float shotSpeedMultiplier = 5.0f;

    [Header("Damage done per shot")]
    [SerializeField] [Range(1, 100)] private float damagePerShot = 50.0f;

    private SCR_PlayerHealth playerHealth;
    private SCR_PlayerInput playerInputScr;

    private bool bMovingTicker = false;
    private bool bMovingBullet = false;
    bool bHitPointActive = false;
    private GameObject latestTicker;
    private GameObject currentTrail;
    private GameObject hitTrail;

    [Header("Audio")]
    [Space(10)]

    [SerializeField] private AudioSource sonarWalkSource,sonarPumpSound, sonarChargeSound, SonarFireSound;

    void Awake()
    {
        cogPrefabs = Resources.Load<SCR_CogPrefabs>("Cog Prefabs");
        sonarAgent = GetComponent<NavMeshAgent>();
        sonarAnim = GetComponentInChildren<Animator>();
        projectileBeam.enabled = false;
        maxHealth = health;
        healthBar.fillAmount = health / maxHealth;
        latestTicker = null;
        chargeUpTimer = sonarChargeSound.clip.length;
    }

    void Update()
    {
        if (started)
        {
            SelectTarget();
            if (currentMode == sonarBehaviourModes.charging)
            {
                Charge();
            }
            else if (currentMode == sonarBehaviourModes.chasing)
            {
                Chase();
            }
            else if (currentMode == sonarBehaviourModes.firing)
            {
                Attack();
            }
            else if (currentMode == sonarBehaviourModes.tickerSpawn)
            {
                TickerSpawnStage();
            }
        }
    }

    public void StartAI()
    {
        SelectTarget();
        sonarAgent.speed = movementSpeed;
        timer = chaseTimer;
        sonarAnim.SetBool("bShieldUp", true);
        sonarAnim.SetBool("bMoving", true);
        currentMode = sonarBehaviourModes.chasing;
        started = true;
        Invoke("StartWalkingSounds", Random.Range(0.0f, 0.25f));
    }

    void StartWalkingSounds()
    {
        sonarWalkSource.Play();
        sonarPumpSound.Play();
    }

    void RotateTurret()
    {
        //Magic rotation code to make turret look at player
        Transform myTransform = turret.transform;
        myTransform.LookAt(currentTarget.transform);
        myTransform.Rotate(Vector3.up, -90.0f);
        myTransform.Rotate(Vector3.forward, -90.0f);
        myTransform.Rotate(Vector3.right, 180.0f);

        turret.transform.rotation = Quaternion.Lerp(turret.transform.rotation, myTransform.rotation, rotationSpeed * Time.deltaTime);
    }

    void DrawLaser()
    {
        RaycastHit hit;
        Vector3 projectileEnd = currentTarget.transform.position;

        Vector3 forward = (turretEndPosition.transform.forward);

        if (Physics.Raycast(turretEndPosition.transform.position, forward, out hit, projectileRange, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
        {
            projectileEnd = hit.point;

        }

        Vector3[] points = new Vector3[] { turretEndPosition.transform.position, projectileEnd };
        projectileBeam.SetPositions(points);
    }

    void FirePulse()
    {
        Vector3 projectileEnd = currentTarget.transform.position;

        GameObject muzzleFlash = Instantiate(sonarMuzzle, turretEndPosition.transform.position, Quaternion.identity);
        muzzleFlash.transform.LookAt(projectileEnd);
        muzzleFlash.transform.Rotate(muzzleFlash.transform.up, -90.0f);
        StartCoroutine(moveBullet(projectileEnd));

        SonarFireSound.PlayOneShot(SonarFireSound.clip);
    }

    IEnumerator moveBullet(Vector3 hitPoint)
    {
        currentTrail = Instantiate(sonarBullet, turretEndPosition.transform.position, Quaternion.identity);
        bMovingBullet = true;

        currentTrail.transform.LookAt(hitPoint);
        Vector3 startPos = turretEndPosition.transform.position + currentTrail.transform.forward * 2.0f;

        float distance = Vector3.Distance(startPos, hitPoint);
        float timerVariable = (distance / 100.0f)* shotSpeedMultiplier;

        float t = 0.0f;

        while (t < timerVariable)
        {
            t += Time.deltaTime;
            currentTrail.transform.position = Vector3.Lerp(startPos, hitPoint, t / timerVariable);
            yield return null;
        }

        if (currentTrail)
        {
            Collider[] hitColliders = Physics.OverlapSphere(currentTrail.transform.position, damageRadius);

            int i = 0;
            while (i < hitColliders.Length)
            {
                //damage players with mDamage
                if (hitColliders[i].gameObject.CompareTag("PlayerTrigger"))
                {
                    playerHealth = hitColliders[i].transform.parent.transform.parent.GetComponent<SCR_PlayerHealth>();
                    if (playerHealth)
                    {
                        playerHealth.DamagePlayer(damagePerShot);
                        //stun code
                        playerInputScr = playerHealth.gameObject.GetComponent<SCR_PlayerInput>();
                        StartCoroutine(playerInputScr.SlowPlayer(stunSpeed, stunTime));
                    }
                }

                i++;
            }

            Destroy(currentTrail);
            bMovingBullet = false;

        }
        bHitPointActive = true;
        hitTrail = Instantiate(sonarHit, hitPoint, Quaternion.identity);
        hitTrail.transform.LookAt(turretEndPosition.transform.position);
        if (hitTrail)
        {
            Destroy(hitTrail, 0.3f);
            bHitPointActive = false;
        }
    }

    public EnemyType ReturnEnemyType()
    {
        return currentEnemyType;
    }

    public void SetRoomManager(GameObject rm)
    {
        roomManager = (IRoom)rm.GetComponent(typeof(IRoom));
        roomManagerObject = rm;
    }

    public void SelectTarget()
    {
        RetreivePlayers();
        if (player1 == null && player2 == null)
        {
            Debug.Log("Both Players Killed");
            this.enabled = false;
        }
        else
        {
            switch (enemyTargetSelectionType)
            {
                case AttackType.Closest:
                    ClosestTarget();
                    break;
                case AttackType.Player1:
                    SetPlayerFirst();
                    break;
                case AttackType.Player2:
                    SetPlayerSecond();
                    break;
                case AttackType.HighestHealth:
                    ;
                    TargetWithHighestHealth();
                    break;
            }
        }
    }

    void RetreivePlayers()
    {
        player1 = GameObject.FindGameObjectWithTag("Player1");
        player2 = GameObject.FindGameObjectWithTag("Player2");
    }

    void SetPlayerFirst()
    {
        currentTarget = player1;
    }

    //function which will target only player 2
    void SetPlayerSecond()
    {
        currentTarget = player2;
    }

    void ClosestTarget()
    {
        currentTarget = null;
        float distance = 100000;

        if (player1)
        {
            if (player1.activeInHierarchy)
            {
                float newDistance = player1.GetComponent<SCR_PlayerInput>().player.GetHealth();
                if (newDistance < distance)
                {
                    distance = newDistance;
                    currentTarget = player1;
                }
            }
        }

        if (player2)
        {
            if (player2.activeInHierarchy)
            {
                float newDistance = player2.GetComponent<SCR_PlayerInput>().player.GetHealth();
                if (newDistance < distance)
                {
                    distance = newDistance;
                    currentTarget = player2;
                }
            }
        }
    }

    //function which will target player with highest health (player 1 priortised in the case of equal)
    void TargetWithHighestHealth()
    {
        currentTarget = null;
        float lowestHealth = 100000;

        if (player1)
        {
            if (player1.activeInHierarchy)
            {
                float health = player1.GetComponent<SCR_PlayerInput>().player.GetHealth();
                if (health < lowestHealth)
                {
                    lowestHealth = health;
                    currentTarget = player1;
                }
            }
        }

        if (player2)
        {
            if (player2.activeInHierarchy)
            {
                float health = player2.GetComponent<SCR_PlayerInput>().player.GetHealth();
                if (health < lowestHealth)
                {
                    currentTarget = player2;
                }
            }
        }
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

        if (playerInputScr)
        {
            playerInputScr.ResetSpeed();
        }

        if (bMovingBullet)
        {
            Destroy(currentTrail);
        }

        if (bHitPointActive)
        {
            Destroy(hitTrail);
        }

        if (bMovingTicker && latestTicker != null)
        {
            latestTicker.GetComponent<IEnemy>().DestroySelf();
        }
        roomManager.RemoveEnemy(this.gameObject);

        if (currentTarget == player1)
        {
            roomManager.updatePlayer1AgentValue(true);
        }
        else if (currentTarget == player2)
        {
            roomManager.updatePlayer2AgentValue(true);
        }
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

    void Chase()
    {
        if (!currentTarget)
        {
            enemyTargetSelectionType = AttackType.Closest;
            SelectTarget();
        }
        sonarAgent.destination = currentTarget.transform.position;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            sonarAnim.SetBool("bMoving", false);

            timer = tickerSpawnTimer;
            sonarAgent.enabled = false;
            bTickerSpawned = false;
            sonarAnim.SetBool("bSpawnTicker", true);
            currentMode = sonarBehaviourModes.tickerSpawn;
        }
    }

    void TickerSpawnStage()
    {
        timer -= Time.deltaTime;

        if (timer < (tickerSpawnTimer - tickerSpawnOffsetDelay) && bTickerSpawned == false)
        {
            bTickerSpawned = true;
            SpawnTicker();
        }

        if (timer <= 0)
        {
            sonarAnim.SetBool("bSpawnTicker", false);
            timer = chargeUpTimer;
            StartCoroutine(RemoveShield(true, shieldInTimer));
            projectileBeam.enabled = true;
            currentMode = sonarBehaviourModes.charging;
            sonarChargeSound.PlayOneShot(sonarChargeSound.clip);
        }
    }

    void Charge()
    {
        RotateTurret();

        DrawLaser();

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            FirePulse();
            projectileBeam.enabled = false;
            timer = firingTimer;
            currentMode = sonarBehaviourModes.firing;
        }
    }

    public void Attack()
    {
        RotateTurret();

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = chaseTimer;
            StartCoroutine(RemoveShield(false, shieldInTimer));
        }
    }

    void SpawnTicker()
    {
        GameObject ticker = Instantiate(tickerPrefab, tickerSpawn.position, Quaternion.identity);
        ticker.GetComponentInChildren<Animator>().SetTrigger("TickerRoll");
        ticker.GetComponent<BoxCollider>().enabled = false;
        ticker.GetComponent<NavMeshAgent>().enabled = false;
        StartCoroutine(MoveTicker(ticker, 0.2f));
    }

    public IEnumerator RemoveShield(bool firing, float spawnTimer)
    {
        float spawnProgress = 0;

        if (firing)
        {
            sonarAnim.SetBool("bShieldUp", false);
        }
        else
        {
            sonarAnim.SetBool("bShieldUp", true);

            shield.GetComponent<BoxCollider>().enabled = true;
            weakZone.GetComponent<BoxCollider>().enabled = false;
        }

        while (spawnProgress < spawnTimer)
        {
            spawnProgress += Time.deltaTime;
            yield return null;
        }

        if (firing)
        {
            shield.GetComponent<BoxCollider>().enabled = false;
            weakZone.GetComponent<BoxCollider>().enabled = true;
        }
        else
        {
            sonarAnim.SetBool("bMoving", true);
            sonarAgent.enabled = true;
            currentMode = sonarBehaviourModes.chasing;

            //magic rotation code to make turret face forward when moving
            Transform myStraightTransform = turret.transform;
            myStraightTransform.LookAt(shield.transform);
            myStraightTransform.Rotate(Vector3.up, -90.0f);
            myStraightTransform.Rotate(Vector3.forward, -90.0f);
            myStraightTransform.Rotate(Vector3.right, 180.0f);
            myStraightTransform.Rotate(Vector3.forward, -60.0f);
        }
    }

    public IEnumerator MoveTicker(GameObject currentTicker, float spawnTimer)
    {
        latestTicker = currentTicker;
        bMovingTicker = true;
        float spawnProgress = 0;
        while (spawnProgress < 1)
        {
            spawnProgress += Time.deltaTime / spawnTimer;
            currentTicker.transform.position = Vector3.Lerp(tickerSpawn.position, tickerExit.position, spawnProgress);
            yield return null;
        }

        currentTicker.GetComponentInChildren<Animator>().SetBool("bUnfolded", true);
        IEnemy currentEnemyInterface = (IEnemy)currentTicker.GetComponent(typeof(IEnemy));
        yield return new WaitForSeconds(1.0f);
        currentEnemyInterface.SetRoomManager(roomManagerObject);
        currentTicker.GetComponent<NavMeshAgent>().enabled = true;
        currentEnemyInterface.StartAI();
        roomManager.AddEnemy(currentTicker);
        currentTicker.GetComponent<BoxCollider>().enabled = true;
        bMovingTicker = false;
        latestTicker = null;
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
        movementSpeed = mSpeed;
    }

    public float GetSpeed()
    {
        return movementSpeed;
    }

    public void BuffEnemy(BuffTypes type)
    {
        if (type != BuffTypes.None)
        {
            healthBar.color = Color.green;
            buffIcon.SetActive(true);
        }
        else
        {
            healthBar.color = Color.red;
            buffIcon.SetActive(false);
        }
    }

    public IEnumerator spawnEnemy(GameObject node, float spawnTimer)
    {
        float spawnProgress = 0;

        Vector3 currentPos = transform.position;
        while (spawnProgress < 1)
        {
            spawnProgress += Time.deltaTime / spawnTimer;
            transform.position = Vector3.Lerp(currentPos, node.transform.position, spawnProgress);
            yield return null;
        }
        node.GetComponent<SCR_SpiderPortal>().ClosePortal(0.5f);
        StartAI();
    }
}
