using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using XboxCtrlrInput;


public class SCR_GruntSpiderAI : MonoBehaviour, IEnemy,IDamageable
{
    [Header("Enemy Type")]
    [SerializeField]
    private EnemyType currentEnemyType;

    private GameObject player1, player2,currentTarget;

    [Header("Target Selection")]
    [SerializeField]
    private AttackType enemyTargetSelectionType;

    [Header("Minimum range values")]
    public float baseEnemyRange, baseAttackRange;

    [SerializeField]
    [HideInInspector]
    private float enemyRange;
    public float setEnemyRange
    {
        get { return enemyRange; }
        set { enemyRange = value; }
    }

    [SerializeField]
    [HideInInspector]
    private float attackRange;
    public float setAttackRange
    {
        get { return attackRange; }
        set { attackRange = value; }
    }

    [SerializeField]
    private float movementSpeed;

    public float setMovementSpeed
    {
        get { return movementSpeed; }
        set { movementSpeed = value; }
    }

    [Header("Attack Time ranges")]
    [SerializeField]
    private float minimumAttackTime, maximumAttackTime;

    private NavMeshAgent gruntAgent;

    private float attackTimer = 0;
    private bool retreating = false;
    private bool attacking = false;

    private Vector3 newPositon;

    private bool spawned = false;

    public Vector3 getRetreatPosition
    {
        get { return newPositon; }
    }
  
    [Header("UI")]
    [SerializeField]
    private Image healthDial;
    [SerializeField] GameObject healthUIObject;
    [SerializeField] private GameObject buffIcon;

    [Header("Health")]
    [SerializeField]
    private float highestHealth;

    private float health;

    [Header("Grunt Type")]
    [SerializeField]
    private GruntTypes currentType;

    [Header("Health bar speed")]
    [SerializeField]
    private float healthBarSpeed;

    [Header("Retreat Range")]
    [SerializeField]
    private float retreatDistanceMultiplier;

    private IRoom roomManager;

    private Vector3 startPosition;

    [Header("Damage to player per hit")]
    [SerializeField]
    private float damagePerHit = 10.0f;

    private float healthMultiplier = 1.0f, speedMultiplier = 1.0f, damagerMultiplier = 1.0f;

    [Header("Buff multiplier value")]
    [SerializeField]
    private float buffMultiplier;

    private int priorityLevel = 50;

    Vector3 offset = Vector3.zero;

    [Header("Cog Drops")]
    [SerializeField] int cogDrops;

    private SCR_CogPrefabs cogPrefabs;

    [Header("Explosion Effect")]
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] float explosionLength = 4.0f;

    private Animator gruntAnim;
    private float maxAnimationSpeed = 1.5f;
    private bool bHasAttacked = false;

    [SerializeField] private AudioSource walkingSource, attackSource;

    [SerializeField]
    Renderer attackRingIndicatorMesh;
    float fullAttackTimer = 0.0f;

    [Header("Attack")]
    [Range(0, 1)] [SerializeField] private float startDamageWindowPerc;
    [Range(0, 1)] [SerializeField] private float endDamageWindowPerc;

    [SerializeField] private BuffTypes current;

    //will return the enemies current type
    public EnemyType ReturnEnemyType()
    {
        return currentEnemyType;
    }

    //function which will set the room manager variable for each grunt
    public void SetRoomManager(GameObject rm)
    {
        roomManager = (IRoom)rm.GetComponent(typeof(IRoom));
    }

    void Awake()
    {
        cogPrefabs = Resources.Load<SCR_CogPrefabs>("Cog Prefabs");
        newPositon = Vector3.zero;
        gruntAgent = GetComponent<NavMeshAgent>();
        gruntAgent.speed = 0;
        gruntAnim = GetComponentInChildren<Animator>();
        health = highestHealth;
        gruntAgent.destination = transform.position;
    }

    //Will start the AI state by selecting a target
    public void StartAI()
    {
        gruntAnim.SetTrigger("Spawned");
        GetComponent<AudioSource>().Play();
        startPosition = transform.position;
        gruntAgent.speed = movementSpeed * speedMultiplier;
        SelectTarget();
        gruntAgent.stoppingDistance = attackRange / 3;
        attackTimer = Random.Range(minimumAttackTime, maximumAttackTime);
        gruntAnim.SetFloat("attackTimerMultiplier", 0.8f / attackTimer); // 0.8f is the length of the animation normally
        fullAttackTimer = attackTimer;
        spawned = true;

        switch (currentType)
        {
            case GruntTypes.Primary: maxAnimationSpeed = 1.8f; break;
            case GruntTypes.Secondary: maxAnimationSpeed = 1.8f; break;
            case GruntTypes.Tertiary: maxAnimationSpeed = 1.3f; break;
            default: maxAnimationSpeed = 1.5f; break;
        }

        float timeOffset = Random.Range(0.000f, 0.2500f);
        Invoke("startWalkingSound", timeOffset);
    }

    void startWalkingSound()
    {
        walkingSource.Play();
    }

    //function which will set a grunts speed and type
    public void SetGrunt(float speed, GruntTypes selectedType , int priority)
    {
        movementSpeed = speed;
        currentType = selectedType;
        priorityLevel = priority;
        gruntAgent.avoidancePriority = priorityLevel;
        gruntAgent.speed = movementSpeed * speedMultiplier;
    }

    //retrieve player objects from the scene
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
                    currentTarget = player2;
                }
            }
        }
    }

    //will select a target based on its attack type
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
                case AttackType.Closest: ClosestTarget(); break;
                case AttackType.Player1: SetPlayerFirst(); break;
                case AttackType.Player2: SetPlayerSecond(); break;
                case AttackType.HighestHealth:; TargetWithHighestHealth(); break;

            }
            CheckTargetViability();
            setOffset();
        }
    }

    void CheckTargetViability()
    { 
        // Makes sure that the selected player is not being attacked too much
        if (player1 && player2 && player1.activeInHierarchy && player2.activeInHierarchy)
        {
            if (currentTarget == player1)
            {
                if (!roomManager.CheckIsPlayerTargetable(true))
                {
                    currentTarget = player2;
                }
            }
            else if (currentTarget == player2)
            {
                if (!roomManager.CheckIsPlayerTargetable(false))
                {
                    currentTarget = player1;
                }
            }
        }

        if (player1)
        {
            if (currentTarget == player1)
            {
                roomManager.updatePlayer1AgentValue(false);
            }
        }

        if (player2)
        {
            if (currentTarget == player2)
            {
                roomManager.updatePlayer2AgentValue(false);
            }
        }
    }

    void setOffset()
    {
        if(currentTarget)
        {
            offset = currentTarget.transform.position - transform.position;
            int rng = Random.Range(0, 2);
            if (rng == 0)
            {
                offset *= -1;
            }
            offset.Normalize();
            offset *= (attackRange / 1.2f);
        }      
    }

    void Update()
    {
        DrawPath();
        if (spawned)
        {
            AgentMovement();
            CheckForAttack();
            float mySpeed = gruntAgent.speed;
            mySpeed = Mathf.Clamp(mySpeed, 0.0f, maxAnimationSpeed);
            gruntAnim.SetFloat("movementSpeed", mySpeed);
        }
        
    }

    void DrawPath()
    {
        NavMeshPath path = new NavMeshPath();
        gruntAgent.CalculatePath(gruntAgent.destination,path);

        for (int i = 0; i < path.corners.Length-1; i++)
        {
            Debug.DrawLine(path.corners[i],path.corners[i+1],Color.green);
        }
    }

    //will check to see if the enemy is within its attack range
    void CheckForAttack()
    {
        if (attacking)
        {
            Attack();
        }
        else
        {
            float distance = 10000;
            if (currentTarget && currentTarget.activeInHierarchy)
            {
               distance = Vector3.Distance(currentTarget.transform.position, transform.position);
            }

            if (distance <= attackRange)
            {
                if (!retreating)
                {
                    gruntAgent.speed = 0;
                    gruntAgent.destination = transform.position;
                    attacking = true;                   
                    gruntAgent.avoidancePriority = 10;
                }
            }
        }
    }

    //function which set enemies destination based on whether it is retreating or attacking
    void AgentMovement()
    {
        if (!attacking)
        {
            if (!retreating)
            {
                if (currentTarget && currentTarget.activeInHierarchy)
                {
                    gruntAnim.SetBool("bMoving", true);
                    gruntAgent.destination = currentTarget.transform.position + offset;
                }
                else
                {
                    gruntAgent.destination = startPosition ;
                    gruntAnim.SetBool("bMoving", true);
                    enemyTargetSelectionType = AttackType.Closest;
                    SelectTarget();
                }
            }
            else if (retreating)
            {
                if (Vector3.Distance(newPositon, gruntAgent.transform.position) < 3.0f)
                {
                    retreating = false;
                    newPositon = Vector3.zero;
                    gruntAnim.SetBool("bMoving", false);
                }
            }
        }        
    }

    //function will remove the enemy from the room manager and destroy itself
    public void DestroySelf()
    {
        roomManager.RemoveEnemy(this.gameObject);

        StartCoroutine(DestructionProcess());
    }

    IEnumerator DestructionProcess()
    {
        float t = 0.0f;

        while (t < 0.12f)
        {
            t += Time.deltaTime;
            float currentScale = Mathf.Lerp(1, 2, t/ 0.12f);
            healthUIObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }
        t = 0.0f;
        while (t<0.2f)
        {
            t += Time.deltaTime;
            float currentScale = Mathf.Lerp(2, 0, t/ 0.2f);
            healthUIObject.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            yield return null;
        }

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

    //function will attack player once its timer is complete
    public void Attack()
    {
        if (!bHasAttacked)
        {
            gruntAnim.SetBool("bMoving", false);
            gruntAnim.SetTrigger("Attack");
            attackSource.PlayScheduled(AudioSettings.dspTime + attackTimer);

            bHasAttacked = true;
        }

        gameObject.transform.LookAt(currentTarget.transform);
        attackTimer -= Time.deltaTime;
        float currentAttackPerc = (fullAttackTimer - attackTimer) / fullAttackTimer;

        Color attackIndicatorColour = Color.Lerp(Color.black, Color.red, (fullAttackTimer - attackTimer) / startDamageWindowPerc);
        attackRingIndicatorMesh.material.SetColor("_EmissionColor", attackIndicatorColour);
        attackRingIndicatorMesh.material.EnableKeyword("_EMISSION");

        if (currentAttackPerc > startDamageWindowPerc && currentAttackPerc < endDamageWindowPerc)
        { // Damage Window

            float distance = Mathf.Infinity;
            if (currentTarget && currentTarget.activeInHierarchy)
            {
                distance = Vector3.Distance(currentTarget.transform.position, transform.position);
            }

            if (distance <= attackRange)
            {
                Vector3 forwardPosition = transform.position + (transform.forward * attackRange);
                float distanceFromForwardPosition = Vector3.Distance(currentTarget.transform.position, forwardPosition);

                if (distanceFromForwardPosition <= attackRange)
                {
                    RaycastHit hit;
                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform.tag == currentTarget.tag)
                        {
                            currentTarget.GetComponent<SCR_PlayerHealth>().DamagePlayer(damagePerHit * damagerMultiplier * Time.deltaTime);
                        }
                    }
                }
            }
        }

        if (attackTimer<=0)
        { // Attack Finished
            gruntAnim.SetBool("bMoving", true);
            
            gruntAgent.avoidancePriority = priorityLevel;
            gruntAgent.speed = 0;
            gruntAgent.enabled = true;                
            RandomPosition();
            attacking = false;
            retreating = true;               
            gruntAgent.speed = movementSpeed * speedMultiplier;
            attackTimer = Random.Range(minimumAttackTime, maximumAttackTime);
            gruntAnim.SetFloat("attackTimerMultiplier", 0.8f / attackTimer); // 0.8f is the length of the animation normally
            fullAttackTimer = attackTimer;
            bHasAttacked = false;
            StartCoroutine(ReturnIndicatorColour());
        }
    }

    IEnumerator ReturnIndicatorColour()
    {
        float t = 0.0f;
        while(t<0.4f)
        {
            t += Time.deltaTime;
            Color attackIndicatorColour = Color.Lerp(Color.red, Color.black,(t/0.4f));
            attackRingIndicatorMesh.material.SetColor("_EmissionColor", attackIndicatorColour);
            attackRingIndicatorMesh.material.EnableKeyword("_EMISSION");
            yield return null;
        }
    }

    //will select a positon for the enemy will retreat to
    void RandomPosition()
    {
        bool pointChosen = false;
        NavMeshPath path = new NavMeshPath();
        while (!pointChosen)
        {
            Vector3 direction = Vector3.zero;
            direction.x = Random.Range(-1.0f, 1.0f);
            direction.z = Random.Range(-1.0f, 1.0f);
            direction.Normalize();
            direction *= enemyRange;

            Vector3 possiblePoint = gruntAgent.transform.position + (direction* retreatDistanceMultiplier);
            gruntAgent.CalculatePath(possiblePoint, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                pointChosen = true;
                newPositon = possiblePoint;
                gruntAgent.destination = possiblePoint;            
            }
        }
    }

    public void DamageEnemy(float bulletDamage)
    {
        UpdateHealth(bulletDamage);
    }

    //will update health based on damage dealt
    public void UpdateHealth(float bulletDamage)
    {
        float newHealth = health * healthMultiplier;
        newHealth -= bulletDamage;
        health = (newHealth / healthMultiplier) * 1;
        healthDial.fillAmount = health/ highestHealth;

        if(health<=0)
        {
            DestroySelf();
        }
     }

    //returns the current health of the enemy
    public float GetHealth()
    {
        return health;
    }

    //will update movement speed based on float passed
    public void SetSpeed(float mSpeed)
    {
        movementSpeed = mSpeed * speedMultiplier;
    }

    //returns the actual speed value of the enemy nav agent
    public float GetSpeed()
    {
        return movementSpeed / speedMultiplier;
    }

    //function which will raise the enemy object into the map based on a timer
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

    // will update the enemies multipliers based on whether it is buffed or not
    public void BuffEnemy(BuffTypes type)
    {
        switch (type)
        {
            case BuffTypes.Health:
                healthMultiplier = buffMultiplier;
                speedMultiplier = 1.0f;
                gruntAgent.speed = movementSpeed;
                damagerMultiplier = 1.0f;
                break;

            case BuffTypes.Damage:
                healthMultiplier = 1.0f;
                speedMultiplier = 1.0f;
                gruntAgent.speed = movementSpeed;
                damagerMultiplier = buffMultiplier;
                break;

            case BuffTypes.Speed:
                healthMultiplier = 1.0f;
                speedMultiplier = buffMultiplier;
                gruntAgent.speed = movementSpeed * speedMultiplier;
                damagerMultiplier = 1.0f;
                break;

            case BuffTypes.None:
                healthMultiplier = 1.0f;
                speedMultiplier = 1.0f;
                gruntAgent.speed = movementSpeed;
                damagerMultiplier = 1.0f;
                break;
        }

        if (type != BuffTypes.None)
        {
            healthDial.color = Color.green;
            buffIcon.SetActive(true);
        }
        else
        {
            healthDial.color = Color.red;
            buffIcon.SetActive(false);
        }

        current = type;
    }
}


[System.Serializable]
public enum GruntTypes
{
    Primary,
    Secondary,
    Tertiary
};

public enum BuffTypes
{
    None,
    Health,
    Speed,
    Damage
};


#if UNITY_EDITOR
[CustomEditor(typeof(SCR_GruntSpiderAI))]
public class SCR_GruntEnemyInspector : Editor
{   
    private SCR_GruntSpiderAI grunt;

    void OnEnable()
    {
        grunt = (SCR_GruntSpiderAI)target;

        if (grunt.setAttackRange <= 0)
        {
            grunt.setAttackRange = grunt.baseAttackRange;
        }


        if (grunt.setEnemyRange <= 0)
        {
            grunt.setEnemyRange = grunt.baseEnemyRange;
        }
       
    }

    void OnSceneGUI()
    {
        Handles.color = Color.blue; 
              
        grunt.setEnemyRange = Handles.RadiusHandle(Quaternion.identity, grunt.transform.position, grunt.setEnemyRange);
        Handles.color = Color.red;
        grunt.setAttackRange = Handles.RadiusHandle(Quaternion.identity, grunt.transform.position, grunt.setAttackRange);      



        Handles.Label(grunt.transform.position + new Vector3(2f, 0.2f, 0), grunt.name);
        Handles.Label(grunt.transform.position + new Vector3(2f, 0f, 0), "Speed:  " + grunt.setMovementSpeed.ToString("f1"));
        Handles.Label(grunt.transform.position + new Vector3(2f, -0.2f, 0), "Maximum Range:  " + grunt.setEnemyRange.ToString("f1"));
        Handles.Label(grunt.transform.position + new Vector3(2f, -0.4f, 0), "Attack Range:  " + grunt.setAttackRange.ToString("f1"));






        if (grunt.getRetreatPosition != Vector3.zero)
        {
            Handles.color = Color.black;
            Handles.RadiusHandle(Quaternion.identity, grunt.getRetreatPosition, 1.0f);
        }


        

    }
}
#endif