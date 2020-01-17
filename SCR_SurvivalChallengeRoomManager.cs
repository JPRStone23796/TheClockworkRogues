using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using UnityEngine;

public class SCR_SurvivalChallengeRoomManager : MonoBehaviour, IRoom, IChallenge
{
    private SCR_RoomInformation roomInformation;

    private List<GameObject> enemyObjects = new List<GameObject>();
    private GameObject player1, player2;

    [Header("Survival")]
    [SerializeField] private float levelLength = 2 * 60;
    private float levelTimer;

    [SerializeField] private List<STR_AddEnemies> enemiesToAdd;
    private Dictionary<EnemyType, int> maxSpawnedEnemies = new Dictionary<EnemyType, int>();
    private Dictionary<EnemyType, List<GameObject>> spawnedEnemies = new Dictionary<EnemyType, List<GameObject>>();

    [Header("Enemy Spawn Delays")]
    [SerializeField] private List<STR_SpawnDelays> spawnDelays;
    private Dictionary<EnemyType, STR_SpawnDelays> enemySpawnDelays = new Dictionary<EnemyType, STR_SpawnDelays>();

    [Header("Parent of every placement node")]
    [SerializeField] private GameObject nodeParent;
    private GameObject[] initialNodes;

    private bool levelStarted, levelEnded = false;
    private int numberOfAgentsTargetingPlayer1 = 0, numberOfAgentsTargetingPlayer2 = 0, PotentialNumberOfAgentsAttackingPlayer1 = 0;

    private GameObject buffEnemy;
    private List<GameObject> grunts;

    [SerializeField] private GameObject platform;
    [SerializeField] private float timeSpeed = 0.25f;
    [SerializeField] private GameObject winningWall;
    [SerializeField]
    private bool player1In = false, player2In = false;
    private Vector3 player1Start, player2Start;

    void Awake()
    {
        roomInformation = Resources.Load<SCR_RoomInformation>("Room Information");
        disableVisualComponentNodes();
        player1 = GameObject.FindGameObjectWithTag("Player1");
        player2 = GameObject.FindGameObjectWithTag("Player2");
        PotentialNumberOfAgentsAttackingPlayer1 = enemyObjects.Count / 2;

        foreach (STR_SpawnDelays s in spawnDelays)
        { // Add to dictionary for easy/quick access
            enemySpawnDelays.Add(s.type, s);
        }

        spawnDelays.Clear();
    }




    public void StartLevel()
    {
        StartCoroutine(LoadLevel());
    }

    public IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(0.7f);
        player1Start = player1.transform.position;
        player2Start = player2.transform.position;
        SCR_UIManager.instance.FadeIn();
        float t = 0;
        Vector3 startPos, player1start, player2start, difference;
        difference = (Vector3.up * 10);
        startPos = platform.transform.position + difference;
        platform.transform.position = startPos;
        player1start = startPos + Vector3.left;
        player2start = startPos + Vector3.right;
        player1start.y += platform.transform.lossyScale.y;
        player2start.y += platform.transform.lossyScale.y;


        player1.transform.position = player1start;
        player2.transform.position = player2start;
        while (t < 1)
        {
            platform.transform.position = Vector3.Lerp(startPos, startPos - difference, t);
            float yHeight = platform.transform.position.y + platform.transform.lossyScale.y;
            player1.transform.position = new Vector3(player1.transform.position.x, yHeight, player1.transform.position.z);
            player2.transform.position = new Vector3(player2.transform.position.x, yHeight, player2.transform.position.z);
            t += Time.deltaTime * timeSpeed;
            yield return null;
        }
        platform.transform.GetChild(0).gameObject.SetActive(false);
        BeginLevel();
    }




    public IEnumerator EndLevel()
    {
        platform.transform.GetChild(0).gameObject.SetActive(true);
        float t = 0;
        Vector3 startPos, difference;
        difference = (Vector3.up * 10);
        startPos = platform.transform.position;
        platform.transform.position = startPos;
        while (t < 1)
        {
            platform.transform.position = Vector3.Lerp(startPos, startPos + difference, t);
            float yHeight = platform.transform.position.y + platform.transform.lossyScale.y;
            player1.transform.position = new Vector3(player1.transform.position.x, yHeight, player1.transform.position.z);
            player2.transform.position = new Vector3(player2.transform.position.x, yHeight, player2.transform.position.z);
            t += Time.deltaTime * timeSpeed;
            if (t >= 1 - (1 * timeSpeed))
            {
                SCR_UIManager.instance.FadeOut();
            }
            yield return null;
        }
        player1.transform.position = player1Start;
        player2.transform.position = player2Start;
        SCR_UIManager.instance.FadeIn();


    }

    
    public IEnumerator releaseWall()
    {
        float t = 0;
        Vector3 startPos, endPos;
        startPos = winningWall.transform.position;
        endPos = startPos;
        endPos.y -= winningWall.transform.lossyScale.y * 1.2f;
        while (t < 1)
        {
            winningWall.transform.position = Vector3.Lerp(startPos, endPos, t);
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator PlayLevel()
    {
        foreach (STR_AddEnemies add in enemiesToAdd)
        {
            if (!maxSpawnedEnemies.ContainsKey(add.type))
            {
                maxSpawnedEnemies.Add(add.type, 0);
                spawnedEnemies.Add(add.type, new List<GameObject>());
            }

            StartCoroutine(AddEnemies(add));
        }

        levelTimer = 0;

        while (levelTimer < levelLength)
        {
            List<EnemyType> keyList = new List<EnemyType>(maxSpawnedEnemies.Keys);

            foreach (EnemyType key in keyList)
            {
                int maxAllowed;

                if (maxSpawnedEnemies.TryGetValue(key, out maxAllowed) && spawnedEnemies[key].Count < maxAllowed)
                {
                    // We can put longer spawn times on different enemy types
                    STR_SpawnDelays waitForSpawn;

                    if (enemySpawnDelays.TryGetValue(key, out waitForSpawn))
                    {
                        StartCoroutine(SpawnEnemies(key, Random.Range(waitForSpawn.delayMin, waitForSpawn.delayMax)));
                    }
                    else
                    {
                        Debug.Log("WARNING: Enemy type " + key + " has not been defined a spawn delay.");
                        StartCoroutine(SpawnEnemies(key, 0));
                    }
                }
            }

            levelTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(releaseWall());
        levelEnded = true;

        yield return null;
    }

    IEnumerator AddEnemies(STR_AddEnemies mToAdd)
    {
        if (mToAdd.timeToAdd > 0)
            yield return new WaitForSeconds(mToAdd.timeToAdd);

        maxSpawnedEnemies[mToAdd.type] = maxSpawnedEnemies[mToAdd.type] + mToAdd.numToAdd;

        yield return null;
    }

    public void BeginLevel()
    {
        levelStarted = true;
        StartCoroutine(PlayLevel());
        StartCoroutine(SCR_UIManager.instance.HideChallengeRoomPrompt());
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player1")
        {
            player1In = true;
        }
        else if (other.gameObject.tag == "Player2")
        {
            player2In = true;
        }
        CheckIfIn();
    }


    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player1")
        {
            player1In = false;
        }
        else if (other.gameObject.tag == "Player2")
        {
            player2In = false;
        }
    }


    void CheckIfIn()
    {
        if (player1In && player2In && levelEnded)
        {
            StartCoroutine(EndLevel());
        }
    }


    IEnumerator SpawnEnemies(EnemyType type, float delay)
    {
        GameObject enemyToBeSpawned = returnEnemyPrefab(type);
        GameObject currentEnemy = Instantiate(enemyToBeSpawned, initialNodes[Random.Range(0,initialNodes.Length)].transform.position, Quaternion.identity);
        enemyObjects.Add(currentEnemy);
        
        //Add to dictionary
        List<GameObject> spawned = new List<GameObject>();

        if (spawnedEnemies.TryGetValue(type, out spawned))
        {
            spawned.Add(currentEnemy);
            spawnedEnemies[type] = spawned;
        }
        else
        {
            spawned.Add(currentEnemy);
            spawnedEnemies.Add(type, spawned);
        }

        IEnemy currentEnemyInterface = (IEnemy)currentEnemy.GetComponent(typeof(IEnemy));
        currentEnemyInterface.SetRoomManager(this.gameObject);

        yield return new WaitForSeconds(delay);

        currentEnemyInterface.StartAI();
        AssignTypes();

        yield return null;
    }

    
    public bool CheckIsPlayerTargetable(bool mIsPlayer1)
    {
        bool possible = false;

        if (mIsPlayer1)
        {
            if (numberOfAgentsTargetingPlayer1 < PotentialNumberOfAgentsAttackingPlayer1)
            {
                possible = true;
            }
        }
        else
        {
            if (numberOfAgentsTargetingPlayer2 < (enemyObjects.Count - PotentialNumberOfAgentsAttackingPlayer1))
            {
                possible = true;
            }
        }

        return possible;
    }
    

    public void updatePlayer1AgentValue(bool mEnemyDead)
    {
        switch (mEnemyDead)
        {
            case true: numberOfAgentsTargetingPlayer1--; break;
            case false: numberOfAgentsTargetingPlayer1++; break;
        }
    }


    public void updatePlayer2AgentValue(bool mEnemyDead)
    {
        switch (mEnemyDead)
        {
            case true: numberOfAgentsTargetingPlayer2--; break;
            case false: numberOfAgentsTargetingPlayer2++; break;
        }
    }


    void disableVisualComponentNodes()
    {

        if (nodeParent != null)
        {
            initialNodes = new GameObject[nodeParent.transform.childCount];
            for (int i = 0; i < nodeParent.transform.childCount; i++)
            {
                GameObject _current = nodeParent.transform.GetChild(i).gameObject;
                initialNodes[i] = _current;
            }

        }
        for (int i = 0; i < initialNodes.Length; i++)
        {
            initialNodes[i].GetComponent<MeshRenderer>().enabled = false;
        }
    }

    //Will assign each grunt enemy within the scene a movement type based upon their distance to the player objects
    void AssignTypes()
    {
        grunts = new List<GameObject>();
        for (int i = 0; i < enemyObjects.Count; i++)
        {
            IEnemy currentEnemyInterface = (IEnemy)enemyObjects[i].GetComponent(typeof(IEnemy));
            EnemyType currentEnemyType = currentEnemyInterface.ReturnEnemyType();

            if (currentEnemyType == EnemyType.Grunt)
            {
                grunts.Add(enemyObjects[i]);
            }
        }
        int sortedGrunts = 0;

        while (grunts.Count > sortedGrunts)
        {
            float distance = 100000.0f;
            int selectedGrunt = 0;

            for (int i = 0; i < grunts.Count; i++)
            {
                Vector3 pos = grunts[i].transform.position;

                float enemyDistance = Vector3.Distance(pos, player1.transform.position);
                if (Vector3.Distance(pos, player2.transform.position) < enemyDistance)
                {
                    enemyDistance = Vector3.Distance(pos, player2.transform.position);
                }

                if (enemyDistance < distance)
                {
                    distance = enemyDistance;
                    selectedGrunt = i;
                }

            }

            if (sortedGrunts < roomInformation.waves.maxmimumSpeed - 1)
            {
                SCR_GruntSpiderAI gruntClass = grunts[selectedGrunt].GetComponent<SCR_GruntSpiderAI>();
                STR_GruntTypes maxSpeedType = roomInformation.gruntSpeedTypes[0];
                gruntClass.SetGrunt(maxSpeedType.speed, maxSpeedType.enemyType, maxSpeedType.priority);
            }
            else if (sortedGrunts >= (roomInformation.waves.maxmimumSpeed - 1) && sortedGrunts < ((roomInformation.waves.maxmimumSpeed - 1) + roomInformation.waves.middleSpeed))
            {
                SCR_GruntSpiderAI gruntClass = grunts[selectedGrunt].GetComponent<SCR_GruntSpiderAI>();
                STR_GruntTypes maxSpeedType = roomInformation.gruntSpeedTypes[1];
                gruntClass.SetGrunt(maxSpeedType.speed, maxSpeedType.enemyType, maxSpeedType.priority);
            }
            else
            {
                SCR_GruntSpiderAI gruntClass = grunts[selectedGrunt].GetComponent<SCR_GruntSpiderAI>();
                STR_GruntTypes maxSpeedType = roomInformation.gruntSpeedTypes[2];
                gruntClass.SetGrunt(maxSpeedType.speed, maxSpeedType.enemyType, maxSpeedType.priority);
            }
            grunts.RemoveAt(selectedGrunt);
            sortedGrunts++;
        }
    }

    //returns a gameObject prefab based on a enemy type, allowing for the correct enemy to be spawned
    GameObject returnEnemyPrefab(EnemyType currentType)
    {
        GameObject enemyPrefab = roomInformation.typesOfEnemies[0];
        for (int j = 0; j < roomInformation.typesOfEnemies.Count; j++)
        {
            IEnemy currentEnemyInterface = (IEnemy)roomInformation.typesOfEnemies[j].GetComponent(typeof(IEnemy));
            EnemyType currentEnemyType = currentEnemyInterface.ReturnEnemyType();

            if (currentEnemyType == currentType)
            {
                enemyPrefab = roomInformation.typesOfEnemies[j];
                break;
            }
        }

        return enemyPrefab;
    }




    public void AddEnemy(GameObject mEnemy)
    {
        enemyObjects.Add(mEnemy);
        IEnemy currentEnemyInterface = (IEnemy)mEnemy.GetComponent(typeof(IEnemy));
        currentEnemyInterface.SetRoomManager(this.gameObject);
        currentEnemyInterface.StartAI();
    }




    public void RemoveEnemy(GameObject mDeadEnemy)
    {
        SCR_GameManager.instance.EnemyKilled();
        enemyObjects.Remove(mDeadEnemy);

        List<GameObject> spawned = new List<GameObject>();
        EnemyType type = mDeadEnemy.GetComponent<IEnemy>().ReturnEnemyType();

        if (spawnedEnemies.TryGetValue(type, out spawned))
        {
            spawned.Remove(mDeadEnemy);
            spawnedEnemies[type] = spawned;
        }
    }

    [System.Serializable]
    public struct STR_AddEnemies
    {
        public float timeToAdd;
        public int numToAdd;
        public EnemyType type;
    }

    [System.Serializable]
    public struct STR_SpawnDelays
    {
        public EnemyType type;
        public float delayMin;
        public float delayMax;

    }
}



