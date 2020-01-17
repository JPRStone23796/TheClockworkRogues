using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;


public class SCR_RoomManager : MonoBehaviour,IRoom
{
    private SCR_RoomInformation roomInformation;
    private List<GameObject> enemyObjects = new List<GameObject>(); 
    private GameObject player1, player2;    
    private List<GameObject> initialNodes = new List<GameObject>();
    private List<GameObject> spawnNodes = new List<GameObject>();
    [Header("Types of enemy room configuration")]
    [SerializeField]
    private List<SCR_RoomConfiguration> roomPossibilitiesList;

    List<GameObject> grunts = new List<GameObject>();
    private SCR_RoomConfiguration selectedConfiguration;


    [Header("Parent of every initial placement node")]
    [SerializeField]
    private GameObject initialNodeParent;

    [Header("Parent of every spawn placement node")]
    [SerializeField]
    private GameObject spawnNodeParent;

    [Header("List containing each room exit")]
    [SerializeField]
    List<GameObject> enterTriggers = new List<GameObject>();


    [Header("Speed in which gates will close")]
    [SerializeField]
    private float gateTimer = 1.0f;

    private bool levelStarted;

    private int numberOfAgentsTargetingPlayer1 = 0, numberOfAgentsTargetingPlayer2 =0, PotentialNumberOfAgentsAttackingPlayer1 = 0;

    private GameObject buffEnemy;


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
            if (numberOfAgentsTargetingPlayer2 < (enemyObjects.Count -PotentialNumberOfAgentsAttackingPlayer1))
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
            case true: numberOfAgentsTargetingPlayer1--;break;
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
    
    void Awake()
    {
        roomInformation = Resources.Load<SCR_RoomInformation>("Room Information");
        disableVisualComponentNodes();

        if (blimpSpotlightLight != null)
        {
            blimpSpotlightLight.gameObject.SetActive(false);
        }
    }

    public void SpawnRoom()
    {
        player1 = GameObject.FindGameObjectWithTag("Player1");
        player2 = GameObject.FindGameObjectWithTag("Player2");
        SpawnLevel();     
        PotentialNumberOfAgentsAttackingPlayer1 = enemyObjects.Count / 2;
    }

    void disableVisualComponentNodes()
    {

        if (initialNodeParent != null)
        {
            for (int i = 0; i < initialNodeParent.transform.childCount; i++)
            {
                GameObject current;
                current = initialNodeParent.transform.GetChild(i).gameObject;
                initialNodes.Add(current);
            }

        }

        if (spawnNodeParent != null)
        {
            for (int i = 0; i < spawnNodeParent.transform.childCount; i++)
            {
                GameObject current;
                current = spawnNodeParent.transform.GetChild(i).gameObject;
                spawnNodes.Add(current);
            }
        }


       
        for (int i = 0; i < initialNodes.Count; i++)
        {
            initialNodes[i].GetComponent<MeshRenderer>().enabled = false;
        }


        /*for (int i = 0; i < spawnNodes.Count; i++)
        {
            //spawnNodes[i].GetComponent<MeshRenderer>().enabled = false;
        }*/

    }



    ///Will spawn a random configuration into the current level based upon the created nodes
    void SpawnLevel()
    {     
        selectedConfiguration = roomPossibilitiesList[Random.Range(0, roomPossibilitiesList.Count)];
        if (initialNodeParent != null)
        {
            for (int i = 0; i < selectedConfiguration.initialEnemyCount; i++)
            {
                STR_InitialSpawnID currentSpawnID = selectedConfiguration.ReturnEnemy(i);

                EnemyType currentType = currentSpawnID.type;
                GameObject enemyToSpawn = returnEnemyPrefab(currentType);
                Vector3 spawnPos = initialNodes[currentSpawnID.node].transform.position;
                spawnPos.y = spawnPos.y + (enemyToSpawn.transform.lossyScale.y / 2);
                GameObject spawn = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                spawn.transform.parent = transform;
                if (currentType != EnemyType.Buff)
                {
                    enemyObjects.Add(spawn);
                }
                else
                {
                    buffEnemy = spawn;
                }

                IEnemy currentEnemyInterface = (IEnemy)spawn.GetComponent(typeof(IEnemy));
                currentEnemyInterface.SetRoomManager(this.gameObject);
            }

        }


        for (int i = 0; i < enterTriggers.Count; i++)
        {
            if (enterTriggers[i] != null)
            {
                enterTriggers[i].GetComponent<IFightRoomTrigger>().setRoomManager(this.gameObject);
            }
        }
    }


    //Will assign each grunt enemy within the scene a movement type based upon their distance to the player objects
    void AssignTypes()
    {       
        grunts= new List<GameObject>();
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

                float enemyDistance = 10000.0f;


                if (player1 != null && player1.activeInHierarchy)
                {
                    enemyDistance = Vector3.Distance(pos, player1.transform.position);
                }

                if (player2 != null && player2.activeInHierarchy)
                {
                    if (Vector3.Distance(pos, player2.transform.position) < enemyDistance)
                    {
                        enemyDistance = Vector3.Distance(pos, player2.transform.position);
                    }
                }
              

                if (enemyDistance < distance)
                {
                    distance = enemyDistance;
                    selectedGrunt = i;
                }

            }

            if (sortedGrunts < roomInformation.waves.maxmimumSpeed-1)
            {
                SCR_GruntSpiderAI gruntClass = grunts[selectedGrunt].GetComponent<SCR_GruntSpiderAI>();
                STR_GruntTypes maxSpeedType = roomInformation.gruntSpeedTypes[0];
                gruntClass.SetGrunt(maxSpeedType.speed, maxSpeedType.enemyType, maxSpeedType.priority);
            }
            else if (sortedGrunts >= (roomInformation.waves.maxmimumSpeed-1) && sortedGrunts < ((roomInformation.waves.maxmimumSpeed-1) + roomInformation.waves.middleSpeed))
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



    //function which will begin the level, blocking the players within the current room until it is completed
    public void BeginLevel()
    {
        if (!levelStarted)
        {
            SCR_Boss.instance.BossAttack(false);
            SCR_AudioManager.instance.Play("DoorClose");
            SCR_AudioManager.instance.PlayFightMusic();
            SCR_GameManager.instance.PlayerFighting = true;
            AssignTargets();
            AssignTypes();         
            StartCoroutine(playLevel());
            MoveWalls();
            levelStarted = true;
        }  
    }


    //move the walls of the room based upon whether the level has been completed
    void MoveWalls()
    {
        for (int i = 0; i < enterTriggers.Count; i++)
        {
            IFightRoomTrigger current = enterTriggers[i].GetComponent<IFightRoomTrigger>();
            current.ClosePrimary(levelStarted, gateTimer);
            if (!levelStarted)
            {
                current.CloseSecondary(true, gateTimer);
            }                     
        }
    }


    public void CloseWalls()
    {
        for (int i = 0; i < enterTriggers.Count; i++)
        {
            if (enterTriggers[i] != null)
            {
                IFightRoomTrigger current = enterTriggers[i].GetComponent<IFightRoomTrigger>();
                current.ClosePrimary(false, 0.0f);
                current.CloseSecondary(false, 0.0f);
            }         
        }
    }


    public void OpenWalls()
    {
        for (int i = 0; i < enterTriggers.Count; i++)
        {
            if (enterTriggers[i] != null)
            {
                IFightRoomTrigger current = enterTriggers[i].GetComponent<IFightRoomTrigger>();
                current.ClosePrimary(true, 0.0f);
                current.CloseSecondary(false, 0.0f);
            }           
        }
    }

    //Set the intially spawned enemies to attack a target
    void AssignTargets()
    {
        for (int i = 0; i < enemyObjects.Count; i++)
        {
            IEnemy currentEnemyInterface = (IEnemy)enemyObjects[i].GetComponent(typeof(IEnemy));
            currentEnemyInterface.StartAI();
        }

        if (buffEnemy != null)
        {
            IEnemy currentEnemyInterface = (IEnemy)buffEnemy.GetComponent(typeof(IEnemy));
            currentEnemyInterface.StartAI();
        }
    }

    //function which will remove an enemy from the current list whenever they are killed
    public void RemoveEnemy(GameObject mDeadEnemy)
    {
        SCR_GameManager.instance.EnemyKilled();
        enemyObjects.Remove(mDeadEnemy);
    }


    public void DebuffEnemies()
    {
        for (int i = 0; i < enemyObjects.Count; i++)
        {
            IEnemy currentEnemyInterface = (IEnemy)enemyObjects[i].GetComponent(typeof(IEnemy));
            currentEnemyInterface.BuffEnemy(BuffTypes.None);
        }
    }



    public void BuffEnemies(Vector3 mWaveOrigin, float mScanDistance)
    {
        BuffTypes currentType = buffEnemy.GetComponent<SCR_BuffSpider>().returnBuffType();
        for (int i = 0; i < enemyObjects.Count; i++)
        {
            float distanceFromWaveOrigin = Vector3.Distance(mWaveOrigin, enemyObjects[i].transform.position);
            if (distanceFromWaveOrigin <= mScanDistance)
            {
                IEnemy currentEnemyInterface = (IEnemy)enemyObjects[i].GetComponent(typeof(IEnemy));
                currentEnemyInterface.BuffEnemy(currentType);
            }
        }          
    }

    [Header("Blimp Spotlight")]
    [SerializeField] PlayableDirector blimpSpotlightPlayableDirector;
    [SerializeField] Light blimpSpotlightLight;
    [SerializeField] float blimpSpotlightFadeOutTime = 2;

    //co routine which will play out the level, spawning in the pooled enemies
    IEnumerator playLevel()
    {
        // Play the blimp spotlight animation if it exists
        if (blimpSpotlightPlayableDirector != null)
        {
            blimpSpotlightLight.gameObject.SetActive(true);
            blimpSpotlightPlayableDirector.Play();
        }
        
        while (selectedConfiguration.poolSize > 0 || enemyObjects.Count > 0)
        {
            if (SCR_Boss.instance.bossAttacking)
            {
                SCR_Boss.instance.BossAttack(false);
            }


            if (enemyObjects.Count < selectedConfiguration.maxSpawnSize && selectedConfiguration.poolSize > 0)
            {
                int rng = Random.Range(0, selectedConfiguration.poolSize);

                bool pointChosen = false;
                int nodeSpawn = 0;
                GameObject currentNode = this.gameObject;
                while (!pointChosen)
                {
                    nodeSpawn = Random.Range(0, spawnNodes.Count);
                    currentNode = spawnNodes[nodeSpawn];
                    if (!currentNode.GetComponent<SCR_SpiderPortal>().currentlySpawning)
                    {
                        pointChosen = true;
                    }
                    yield return null;
                }
                currentNode.GetComponent<SCR_SpiderPortal>().OpenPortal(0.5f);
                GameObject spawn = Instantiate(returnEnemyPrefab(selectedConfiguration.ReturnPoolsType(rng)), currentNode.transform.position + (Vector3.down * 3), Quaternion.identity);
                spawn.transform.parent = transform;
                enemyObjects.Add(spawn);
                AssignTypes();
                IEnemy currentEnemyInterface = (IEnemy)spawn.GetComponent(typeof(IEnemy));
                currentEnemyInterface.SetRoomManager(this.gameObject);
                if (buffEnemy != null)
                {
                currentEnemyInterface.BuffEnemy(buffEnemy.GetComponent<SCR_BuffSpider>().returnBuffType());
                }                 
                float timer = Random.Range(2.5f, 4.0f);
                StartCoroutine(currentEnemyInterface.spawnEnemy(currentNode, timer));
            }
            yield return null;
        }

        SCR_AudioManager.instance.Play("DoorOpen");
        for (int i = 0; i < enterTriggers.Count; i++)
        {
            IFightRoomTrigger current = enterTriggers[i].GetComponent<IFightRoomTrigger>();
            current.ClosePrimary(true, gateTimer);
        }

        if (buffEnemy != null)
        {
            buffEnemy.GetComponent<SCR_BuffSpider>().PowerDown();
        }
        SCR_AudioManager.instance.PlayWalkMusic();
        SCR_GameManager.instance.PlayerFighting = false;

        if (player1 & player2)
        {
            SCR_PlayerWorldSpaceUI player1WorldSpaceUIScr = player1.GetComponent<SCR_PlayerWorldSpaceUI>();
            SCR_PlayerWorldSpaceUI player2WorldSpaceUIScr = player2.GetComponent<SCR_PlayerWorldSpaceUI>();

            if (player1WorldSpaceUIScr)
            {
                player1WorldSpaceUIScr.HealPrompt(true);
            }

            if (player2WorldSpaceUIScr)
            {
                player2WorldSpaceUIScr.HealPrompt(true);
            }

            yield return new WaitForSeconds(4.0f);

            if (player1WorldSpaceUIScr)
            {
                player1WorldSpaceUIScr.HealPrompt(false);
            }

            if (player2WorldSpaceUIScr)
            {
                player2WorldSpaceUIScr.HealPrompt(false);
            }
        }

        // Fade out the blimp light
        if (blimpSpotlightLight != null)
            StartCoroutine(FadeOutBlimpSpotlight());
    }

    IEnumerator FadeOutBlimpSpotlight()
    {
        float time = 0;
        float startIntensity = blimpSpotlightLight.intensity;
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        while (time < blimpSpotlightFadeOutTime)
        {
            time += Time.deltaTime;
            blimpSpotlightLight.intensity = Mathf.SmoothStep(startIntensity, 0, time / blimpSpotlightFadeOutTime);
            yield return wait;
        }

        blimpSpotlightPlayableDirector.Stop();
    }


    public void AddEnemy(GameObject mEnemy)
    {
        enemyObjects.Add(mEnemy);
        IEnemy currentEnemyInterface = (IEnemy)mEnemy.GetComponent(typeof(IEnemy));
        currentEnemyInterface.SetRoomManager(this.gameObject);
        currentEnemyInterface.StartAI();
    }


}

[System.Serializable]
//struct which stores the types of grunts possible
public struct STR_GruntTypes
{
    public float speed;
    public GruntTypes enemyType;
    public int priority;
}


[System.Serializable]
//struct which stores how many of each type of grunt can exist
public struct STR_GruntWaves
{
    public int maxmimumSpeed;
    public int middleSpeed;
}

