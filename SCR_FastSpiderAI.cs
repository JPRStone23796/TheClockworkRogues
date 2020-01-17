using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
using XboxCtrlrInput;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SCR_FastSpiderAI : MonoBehaviour,IEnemy,IDamageable
{
    [Header("Enemy Type")]
    [SerializeField]
    private EnemyType currentEnemyType;

    private GameObject player1, player2, currentTarget;


    [Header("Target Selection")]
    [SerializeField]
    private AttackType enemyTargetSelectionType;

    private float buffMultiplier = 1f;
    private float speedMultiplier = 1f;
    public float setbuffMultiplier
    {
        get { return buffMultiplier; }
        set { buffMultiplier = value; }
    }

    private IRoom roomManager;

    private float health = 1;


    [Header("AI Movement Speed")]
    [Range(10,40)]
    [SerializeField]
    private float movementSpeed;
    public float setMovementSpeed
    {
        get { return movementSpeed; }
        set { movementSpeed = value; }
    }

    private NavMeshAgent tickerAgent;
    
    private int pathDirectionType = 0;

    private Vector3 lastSeenPos = Vector3.zero;
    private float initialHeight = 0;

    [Header("The maximum number of points along the AI's initial curved path")]
    [SerializeField]
    [Range(5, 20)]
    private int initialPathPoints = 10;

    private int currentPathPoints = 0;

    [SerializeField]
    [HideInInspector]
    private float tickerRange = 10.0f;

    public float getTickerRange
    {
        get { return tickerRange; }
        set { tickerRange = value; }
    }

    [HideInInspector] public float minimumTickerRange = 1.0f;

    [SerializeField]
    private float explosionRadius = 10.0f;

    public float getExplosionRange
    {
        get { return explosionRadius; }
        set { explosionRadius = value; }
    }
    [HideInInspector]
    public float minimumExplosionRange = 1.0f;

    [Header("Maximum amount of damage dealt to the player")]
    
    [Range(0, 100)] [SerializeField] private float maximumExplosionDamage = 10;
    [Range(0, 100)] [SerializeField] float minimumExplosionDamage = 5;

    [Header("Time before the ticker will explode after reaching a target")]
    [Range(0.1f,3.0f)]
    [SerializeField] float timeToExplode = 0.7f;

    [Header("Range from player objects where the enemy will which targets")]
    [SerializeField]
    [Range(5, 20)]
    private float playerRange = 10.0f;

    [Header("Ticker Stopping Range (Distance to player destruction starts)")]
    [SerializeField] float stoppingDistance = 1f;

    [Header("Cog Drops")]
    [SerializeField] int cogDrops;

    [Header("Explosion Effect")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionLength = 4.0f;

    [Header("UI")]
    [SerializeField] private Image healthBar;

    [SerializeField] private GameObject healthBarObject;
    [SerializeField] private GameObject buffIcon;


    [Header("Audio")]
    [SerializeField] private AudioSource alarmAudioSource;

    private bool movementStarted = false;
    private SCR_CogPrefabs cogPrefabs;

    private Animator tickerAnim;
    private bool bSpawned = false;

    private bool _isExploding = false;

    List<Vector3> points = new List<Vector3>();
    private int currentpoint = 1;

    private bool near = false;

    [Header("Layers that will block explosion damage")]
    [SerializeField] private LayerMask blockDamageLayer;

    void Awake()
    {
        cogPrefabs = Resources.Load<SCR_CogPrefabs>("Cog Prefabs");
        tickerAgent = GetComponent<NavMeshAgent>();
        tickerAnim = GetComponentInChildren<Animator>();
        pathDirectionType = Random.Range(0, 2);
        currentPathPoints = initialPathPoints;
        UpdateHealthUI();
}

    public EnemyType ReturnEnemyType()
    {
        return currentEnemyType;
    }

    public void SetRoomManager(GameObject rm)
    {
        roomManager = (IRoom)rm.GetComponent(typeof(IRoom));
    }

    public void SelectTarget()
    {
        RetreivePlayers();
        if (player1 == null && player2 == null)
        {
            Debug.Log("Both Players Killed");
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

    //function which will target only player 1
    void SetPlayerFirst()
    {
        currentTarget = player1;
    }

    //function which will target only player 2
    void SetPlayerSecond()
    {
        currentTarget = player2;
    }

    //function which will  target its closest player (player 1 priortised in the case of equal)
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
                    lowestHealth = health;
                    currentTarget = player2;
                }
            }
        }
    }

    void CheckDistance()
    {
        if (player1)
        {
            if (player1.activeInHierarchy)
            {
                if (currentTarget != player1)
                {
                    float distance = Vector3.Distance(player1.transform.position, transform.position);

                    if (distance <= playerRange)
                    {
                        currentTarget = player1;
                        FindPath(1.4f);
                    }
                }

            }
        }



        if (player2)
        {
            if (player2.activeInHierarchy)
            {
                if (currentTarget != player2)
                {
                    float distance = Vector3.Distance(player2.transform.position, transform.position);

                    if (distance <= playerRange)
                    {
                        currentTarget = player2;
                        FindPath(1.4f);
                    }
                }

            }
        }
    }
    
    public void DestroySelf()
    { // This function does not destroy the GameObject as
        // in the ticker challenge room they are pooled (SetActive(false))
        StartCoroutine(DestructionProcess());
    }

    /// <summary>
    /// The explosion is telegraphed through the animation of the health bar.
    /// The duration of this animation = timeToExplode, then the damage is dealt.
    /// </summary>
    IEnumerator DestructionProcess()
    {
        // Stop ticker
        tickerAgent.destination = transform.position;
        tickerAgent.speed = 0;

        // Inflate health bar
        float t = 0.0f;
        float duration = timeToExplode * 0.375f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float currentScale = Mathf.Lerp(1, 2, t / 0.12f);
            healthBarObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }
        //

        // Deflate health bar
        t = 0.0f;
        duration = timeToExplode * 0.625f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float currentScale = Mathf.Lerp(2, 0, t / 0.2f);
            healthBarObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }
        //

        if (currentTarget == player1)
        {
            roomManager.updatePlayer1AgentValue(true);
        }
        else if (currentTarget == player2)
        {
            roomManager.updatePlayer2AgentValue(true);
        }

        // Damage Players here because this is where the explosion starts
        Attack();

        // Spawn cogs
        SpawnCogs();

        Vector3 explosionPos = transform.position;

        //TODO  what if a level is at a different height?
        explosionPos.y = 0.1f;
         
        GameObject finalExplosion = Instantiate(explosionPrefab, explosionPos, Quaternion.identity);

        if (roomManager is SCR_TickerChallengeRoomManager)
        { // if its the challenge room they are pooled, so they cannot be destroyed
            roomManager.RemoveEnemy(gameObject);
            yield break;
        }

        Destroy(finalExplosion, explosionLength);
        Destroy(gameObject);

        roomManager.RemoveEnemy(gameObject);
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

    public void Attack()
    {
        Collider[] hitPlayers;

        // This has been changed to an overlap sphere, a SphereCast doesn't make sense to use
        hitPlayers = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var explodedObjects in hitPlayers)
        {
            if (explodedObjects.transform.gameObject.tag == "Player1" || explodedObjects.transform.gameObject.tag == "Player2")
            {
                if (!Physics.Raycast(transform.position, // Raycasts from ticker to enemy, checking if any geometry blocks the damage
                                    explodedObjects.transform.position - transform.position, 
                                    Vector3.Distance(transform.position, explodedObjects.transform.position),
                                    blockDamageLayer, 
                                    QueryTriggerInteraction.Ignore))
                {
                    float hitDistance = Vector3.Distance(transform.position, explodedObjects.transform.position);
                    float percentage = 1 - (hitDistance / explosionRadius); // Needs to be reversed (1 - x) so futher away -> lower value
                    float damageDealt = Mathf.Lerp(minimumExplosionDamage, maximumExplosionDamage, percentage);
                    explodedObjects.transform.GetComponent<SCR_PlayerHealth>().DamagePlayer(damageDealt);
                }
            }
        }
    }

    void FindPath( float multiplier)
    {
        Vector3 startPos = transform.position;
        if (!currentTarget)
        {
            enemyTargetSelectionType = AttackType.Closest;
            SelectTarget();
        }
        Vector3 endPos = currentTarget.transform.position;
        points = new List<Vector3>();
        currentpoint = 0;

        if (Vector3.Distance(startPos, endPos) >= tickerRange)
        {
            Vector3 midPoint = endPos - ((endPos - startPos) / 2);
            lastSeenPos = endPos;
            float nextZ, otherZ;

            if (pathDirectionType == 0)
            {
                midPoint.x += Random.Range(-1, 2);
                midPoint.z = startPos.z + ((startPos.z - endPos.z) * multiplier);
                nextZ = midPoint.z;
                otherZ = endPos.z - ((startPos.z - endPos.z) * multiplier);
            }
            else
            {
                midPoint.x += Random.Range(-1, 2);
                midPoint.z = endPos.z - ((startPos.z - endPos.z) * multiplier);
                nextZ = midPoint.z;
                otherZ = endPos.z + ((startPos.z - endPos.z) * multiplier);
            }

            float currentDistance = Vector3.Distance(transform.position, currentTarget.transform.position);
            float percentage = currentDistance / initialHeight;
            int pathPoints = (int)(initialPathPoints * percentage);
            pathPoints = Mathf.Clamp(pathPoints, 1, initialPathPoints);
            float increment = (1.0f / pathPoints);

            Vector3 test = midPoint;

            float t = 0f;
          
            while (t < 1)
            {
                float value = Mathf.Lerp(nextZ, otherZ, t);
                test.z = value;
                points = new List<Vector3>();
                bool onpath = true;

                for (float j = 0+increment; j <= 1; j += increment)
                {
                    points.Add(calculateBezierPoint(j, startPos, test, endPos));
                }

                for (int z = 0; z < points.Count; z++)
                {
                    NavMeshHit hit;
                    if (!NavMesh.SamplePosition(points[z], out hit, 0.5f, NavMesh.AllAreas))
                    {
                        onpath = false;
                    }
                  
                }

                if (onpath)
                {
                    t = 2;
                }
                else
                {
                    t += 0.05f;
                }
            }
            points.Add(endPos);
            CheckPath();
            tickerAgent.destination = points[currentpoint];
        }
        else
        {
            points.Add(endPos);
            tickerAgent.destination = points[currentpoint];
        }
    }

    void CheckPath()
    {
        points.Reverse();

        float playerDistance = Vector3.Distance(transform.position, currentTarget.transform.position);
        List<Vector3> sortedPoints = new List<Vector3>();

        for (int i = 0; i < points.Count; i++)
        {
            float distance = Vector3.Distance(currentTarget.transform.position , points[i]);

            if (distance <= playerDistance)
            {
                sortedPoints.Add(points[i]);
            }
            else
            {
                break;
            }
        }
        sortedPoints.Reverse();
        points = sortedPoints;
    }

    private Vector3 calculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    void Update()
    {
        if (movementStarted && gameObject && !_isExploding)
        {
            CheckDistance();
            if (!near)
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    //Debug.DrawLine(points[i], points[i + 1], Color.green);
                }

                if (Vector3.Distance(lastSeenPos, currentTarget.transform.position) >= 3.5f)
                {
                    currentpoint = 1;
                    FindPath(1.4f);
                }

                if (Vector3.Distance(transform.position, currentTarget.transform.position) >= stoppingDistance)
                {
                    if (currentpoint < points.Count)
                    {
                        tickerAgent.destination = points[currentpoint];
                        if (Vector3.Distance(transform.position, points[currentpoint]) < 3.5f)
                        {
                            currentpoint++;
                        }
                    }
                }
                else if (!_isExploding)
                {
                    near = true;
                    tickerAgent.destination = transform.position;
                    TickerCountDown();
                }
            }
        }
    }

    void TickerCountDown()
    {
        _isExploding = true;
        alarmAudioSource.Play();
        DestroySelf();
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

    private void UpdateHealthUI()
    {
        healthBar.fillAmount = 1;
    }

    public float GetHealth()
    {
        return health;
    }

    public void SetSpeed(float mSpeed)
    {
        tickerAgent.speed *= mSpeed;
    }

    public float GetSpeed()
    {
        return tickerAgent.speed;
    }

    public void StartAI()
    {
        if (gameObject)
        {
            SelectTarget();
            tickerAgent.speed = movementSpeed * speedMultiplier;
            if (!tickerAnim.GetBool("bUnfolded"))
            {
                tickerAnim.SetTrigger("TickerRoll");
                tickerAnim.SetBool("bUnfolded", true);
            }
            if (currentTarget)
            {
                initialHeight = Vector3.Distance(transform.position, currentTarget.transform.position);
                movementStarted = true;
                FindPath(2.0f);
            }
        }
    }

    public void SetSpeedMultiplier(float mNewMultiplier)
    {
        speedMultiplier = mNewMultiplier;
        tickerAgent.speed = movementSpeed * speedMultiplier;
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
            if (gameObject != null && health > 0)
            {
                spawnProgress += Time.deltaTime / spawnTimer;
                transform.position = Vector3.Lerp(currentPos, node.transform.position, spawnProgress);
                yield return null;
            }
            else
            {
                yield break;
            }
        }
        if (gameObject)
        {
            node.GetComponent<SCR_SpiderPortal>().ClosePortal(0.5f);
            StartAI();
            bSpawned = true;
        }
    }

    public void SetCogsDropped(int cogs)
    {
        cogDrops = cogs;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SCR_FastSpiderAI))]
public class SCR_TickerEnemyInspector : Editor
{
    private SCR_FastSpiderAI ticker;

    void OnEnable()
    {
        ticker = (SCR_FastSpiderAI)target;

        if (ticker.getTickerRange <= 0)
        {
            ticker.getTickerRange = ticker.minimumTickerRange;
        }

        if (ticker.getExplosionRange <= 0)
        {
            ticker.getExplosionRange = ticker.minimumExplosionRange;
        }
    }

    void OnSceneGUI()
    {
        Handles.color = Color.blue;
        ticker.getTickerRange = Handles.RadiusHandle(Quaternion.identity, ticker.transform.position, ticker.getTickerRange);

        Handles.color = Color.red;
        ticker.getExplosionRange = Handles.RadiusHandle(Quaternion.identity, ticker.transform.position, ticker.getExplosionRange);

        Handles.Label(ticker.transform.position + new Vector3(2f, 0.2f, 0), ticker.name);
        Handles.Label(ticker.transform.position + new Vector3(2f, 0f, 0), "Speed:  " + ticker.setMovementSpeed.ToString("f1"));
        Handles.Label(ticker.transform.position + new Vector3(2f, -0.2f, 0), "Maximum Range:  " + ticker.getTickerRange.ToString("f1"));
        Handles.Label(ticker.transform.position + new Vector3(2f, -0.4f, 0), "Attack Range:  " + ticker.getExplosionRange.ToString("f1"));
    }
}
#endif