using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class SCR_Boss : MonoBehaviour
{

    internal static SCR_Boss instance = null;

 
[Header("Boss Manager which will attack player through the alleyways. Will need a BossManager Tag on the object")]

    [Space(8)]
    [Tooltip("Bomb Prefab")]
   [SerializeField] private GameObject Bomb;

    public XboxController controller;


    [SerializeField] private XboxButton buttonPress = XboxButton.Start;

    [Space(8)]
    [Tooltip("Range Values for the attack pattern timer")]
   [SerializeField] private float minimumTimerRange, maximumTimerRange;

    [Space(8)]
    [Tooltip("Timer used to determine when to attack an idle player")]
   [SerializeField] private float idleTimer;

    [Space(8)]
    [Tooltip("Will Determine how many bombs will be dropped every attack cycle")]
    [Range(5, 20)] [SerializeField] private int bombsPerRun;

    [Space(8)]
    [Tooltip("How quickly will the bomb indicator will scale out")]
    [SerializeField] private float scaleTimer = 0.2f;

    [Space(8)]
    [Tooltip("Timer used to indicate how quickly the bomb will land")]
    [SerializeField] private float timeToLand = 1.0f;

    [Space(8)]
    [Tooltip("value used to determine how far away each bomb will be from each other")]
    [SerializeField]
    private float bombRatio = 2.5f;

    [Space(8)]
    [Tooltip("time between bomb drops during the cluster bomb attack")]
    [SerializeField]
    float timerBetweenClusterAttacks = 1.0f;

    [Space(8)]
    [Tooltip("time between bomb drops during the strafe attacks")]
    [SerializeField]
    float timeBetweenStrafeBombAttacks = 0.12f;

    [Space(8)]
    [Tooltip("value used to indicate if the boss can attack")]
    [SerializeField] private bool canAttack = false;

    public bool bossAttacking
    {
        get { return canAttack; }
    }

    private GameObject player1, player2;
    private SCR_PlayerInput player1Script, player2Script;

    private float _Player1IdleTimer = 0.0f, _Player2IdleTimer = 0.0f, _CountdownTimer = 0.0f, _currentBombTimer = 0.0f;

    private Vector3 _currentPlayer1Pos, _currentPlayer2Pos;

    private List<Vector3> bombPositions;

    private bool placingBombs = false, player1AttackIdle = false, player2AttackIdle = false, bossDestroyed  = false;

  


    private IEnumerator currentAttack;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }



    void Start()
    {      
        player1 = GameObject.FindGameObjectWithTag("Player1");
        player2 = GameObject.FindGameObjectWithTag("Player2");
        player1Script = player1.GetComponent<SCR_PlayerInput>();
        player2Script = player2.GetComponent<SCR_PlayerInput>();
        _currentBombTimer = Random.Range(minimumTimerRange,maximumTimerRange);
        _currentPlayer1Pos = player1.transform.position;
        _currentPlayer2Pos = player2.transform.position;
    }

    void Update()
    {
        if (!bossDestroyed)
        {
            if (canAttack)
            {
                UpdateTimers();
                CheckTimers();
            }

            if (!player1.activeInHierarchy && !player2.activeInHierarchy)
            {
                canAttack = false;
            }
        } 
        
    }



    public void BossDefeated()
    {
        canAttack = false;
        bossDestroyed = true;

    }

    //updates all countdown timers 
    void UpdateTimers()
    {
        _CountdownTimer += Time.deltaTime;


        if (player1.activeInHierarchy && Vector3.Distance(player1.transform.position, _currentPlayer1Pos) <= 0.1f)
        {
            _Player1IdleTimer += Time.deltaTime;
        }
        else
        {
            _Player1IdleTimer = 0.0f;
        }

        if (player2.activeInHierarchy && Vector3.Distance(player2.transform.position, _currentPlayer2Pos) <= 0.1f)
        {
            _Player2IdleTimer += Time.deltaTime;
        }
        else
        {
            _Player2IdleTimer = 0.0f;
        }
      
    }


    //will check for the idle timers and attack timers, if they have reached 0 then the relavant attack pattern will be called
    void CheckTimers()
    {
        CheckPlayerIdle();
        if (!placingBombs)
        {          
            if (_CountdownTimer >= _currentBombTimer)
            {
                _Player2IdleTimer = 0.0f;
                _Player1IdleTimer = 0.0f;
                placingBombs = true;

                int attackPattern = Random.Range(0, 3);

                if (attackPattern < 2)
                {
                    currentAttack = SpawnRun(attackPattern);
                }
                else
                {
                    currentAttack = ClusterAttack();
                }
                StartCoroutine(currentAttack);
                _CountdownTimer = 0.0f;
                _currentBombTimer = Random.Range(minimumTimerRange, maximumTimerRange);
            }

            _currentPlayer1Pos = player1.transform.position;
            _currentPlayer2Pos = player2.transform.position;
        }
    }



    //public function that will be used to stop the bosses attacks while in certain rooms
    public void BossAttack(bool mIsAttacking)
    {
        canAttack = mIsAttacking;
        _CountdownTimer = 0.0f;
        _Player2IdleTimer = 0.0f;
        _Player2IdleTimer = 0.0f;
        placingBombs = false;
        player1AttackIdle = false;
        player2AttackIdle = false;
        if (!mIsAttacking)
        {
            if (currentAttack != null)
            {
                StopCoroutine(currentAttack);
            }
            DestroyCurrentBombs();
        }
        currentAttack = null;
    }




    //will place a bomb on idle player
    void CheckPlayerIdle()
    {
        if (player1.activeInHierarchy &&  _Player1IdleTimer >= idleTimer && !player1AttackIdle)
        {
            StartCoroutine(PlaceBomb(player1.transform.position));
            _currentPlayer1Pos = player1.transform.position;
            _Player1IdleTimer = 0.0f;
            player1AttackIdle = true;
            placingBombs = true;
            Invoke("ResetPlayer1Timer", timeToLand);
   
        }

        if (_Player2IdleTimer >= idleTimer && !player2AttackIdle)
        {
            StartCoroutine(PlaceBomb(player2.transform.position));

            _currentPlayer1Pos = player1.transform.position;
            _Player2IdleTimer = 0.0f;
            player2AttackIdle = true;
            placingBombs = true;
            Invoke("ResetPlayer2Timer", timeToLand);

        }
    }

    //resets timers and values once the attack has stopped
    void ResetPlacingBombs()
    {
        _Player2IdleTimer = 0.0f;
        _Player1IdleTimer = 0.0f;
        _CountdownTimer = 0.0f;
        placingBombs = false;
    }

    void ResetPlayer1Timer()
    {
        _Player1IdleTimer = 0.0f;
        player1AttackIdle = false;
        placingBombs = false;
    }

    void ResetPlayer2Timer()
    {
        _Player2IdleTimer = 0.0f;
        player2AttackIdle = false;
        placingBombs = false;
    }



    //will select a random player transform from those available
    Transform SelectPlayer()
    {
        int selectedPlayer = Random.Range(0, 2);


        Transform currentTarget = null;
        bool targetChosen = false;

        while (!targetChosen)
        {
            selectedPlayer = Random.Range(0, 2);
            if (selectedPlayer == 1 && player2.activeInHierarchy)
            {
                currentTarget = player2.transform;
                targetChosen = true;
            }
            if (selectedPlayer == 0 && player1.activeInHierarchy)
            {
                currentTarget = player1.transform;
                targetChosen = true;
            }
        }

        return currentTarget;

    }


    //cluster attack co-routine which will drop bombs on both players locations quickly
    IEnumerator ClusterAttack()
    {

        Transform currentTarget = SelectPlayer();
        int bombsDropped = 0;
        Vector3 targetPos = currentTarget.position;
        Vector3 potentialNoise = new Vector3(CalculateNoiseValue(2, 4), 0, CalculateNoiseValue(2, 4));
        targetPos += potentialNoise;
        targetPos.Normalize();

        while (bombsDropped < bombsPerRun)
        {

            Vector3 newTarget = currentTarget.position + (targetPos * 1.5f);
            StartCoroutine(PlaceBomb(newTarget));
            bombsDropped++;
            float angleChange = Random.Range(35, 70);
            targetPos = Quaternion.AngleAxis(angleChange, Vector3.up) * targetPos;
            currentTarget = SelectPlayer();
            yield return  new WaitForSeconds(timerBetweenClusterAttacks);
        }
        Invoke("ResetPlacingBombs", 0.0f);

    }




    //co-routine which will determine a strafe pattern between the players and then either carry out straight line strafe, or will add noise to points
    IEnumerator SpawnRun(int type)
    {
        bombPositions = new List<Vector3>();


        Vector3 player1NextPos = Vector3.zero, player2NextPos = Vector3.zero, centralPoint = Vector3.zero;
        float speed = 0.0f;

        if (player1.activeInHierarchy)
        {
            speed = player1Script.maxMoveSpeed / 2;
            player1NextPos = (player1.transform.position - _currentPlayer1Pos).normalized;
            if (Vector3.Distance(player1.transform.position, _currentPlayer1Pos) <= 0.2f)
            {
                speed = 1f;
                player1NextPos = (player1.transform.position - (player1.transform.forward / 10)).normalized;
            }
            player1NextPos = player1.transform.position + (player1NextPos * speed * timeToLand);
        }


        if (player2.activeInHierarchy)
        {
            speed = player2Script.maxMoveSpeed/2;
            player2NextPos = (player2.transform.position - _currentPlayer2Pos).normalized;
            if (Vector3.Distance(player2.transform.position, _currentPlayer2Pos) <= 0.2f)
            {
                speed = 1f;
                player2NextPos = (player2.transform.position - (player2.transform.forward / 10)).normalized;
            }
            player2NextPos = player2.transform.position + (player2NextPos * speed * timeToLand);
        }


        if (player1.activeInHierarchy && player2.activeInHierarchy)
        {
            centralPoint = player1NextPos + ((player2NextPos - player1NextPos) / 2);

            Vector3 playerDirection = (centralPoint - player1NextPos).normalized;
            player1NextPos = centralPoint + (playerDirection * (bombRatio * bombsPerRun));

            playerDirection = (centralPoint - player2NextPos).normalized;
            player2NextPos = centralPoint + (playerDirection * (bombRatio * bombsPerRun));
        }
        else if (player1.activeInHierarchy && !player2.activeInHierarchy)
        {
            centralPoint = player1.transform.position;
            Vector3 playerDirection = (centralPoint - player1NextPos).normalized;
            player1NextPos = centralPoint + (playerDirection * (bombRatio * bombsPerRun));
            player2NextPos = centralPoint - (playerDirection * (bombRatio * bombsPerRun));
        }
        else if (!player1.activeInHierarchy && player2.activeInHierarchy)
        {
            centralPoint = player2.transform.position;
            Vector3 playerDirection = (centralPoint - player2NextPos).normalized;
            player1NextPos = centralPoint + (playerDirection * (bombRatio * bombsPerRun));
            player2NextPos = centralPoint - (playerDirection * (bombRatio * bombsPerRun));
        }


       

        bombPositions.Add(player1NextPos);
        float t = 0.0f;
        float stages = 1 / (float) (bombsPerRun-1);
        while (t < (1-stages))
        {
            t += stages;
            Vector3 nextPos = Vector3.Lerp(player1NextPos, player2NextPos, t);
            bombPositions.Add(nextPos);
        }
        bombPositions.Add(player2NextPos);
        Invoke("ResetPlacingBombs", timeToLand+2);
        for (int i = 0; i < bombPositions.Count; i++)
        {
            Vector3 currentPos = bombPositions[i];
            currentPos.y = 0.1f + (0.01f * i);
            bombPositions[i] = currentPos;

            if (type == 1)
            {
                Vector3 previous = Vector3.zero;
                if (i > 0)
                {
                    previous = bombPositions[i - 1];
                }
                StartCoroutine(PlaceBombWNoise(bombPositions[i], previous));
            }
            else if (type == 0)
            {
                StartCoroutine(PlaceBomb(bombPositions[i]));
            }
          
            yield return new WaitForSeconds(timeBetweenStrafeBombAttacks);
        }
      

    }




    //function to calculate noise value to be used for each point 
    Vector3 Noise(Vector3 mPreviousPoint, Vector3 mCurrentPoint)
    {
        Vector3 noiseValue = Vector3.zero;
        if (mPreviousPoint != Vector3.zero)
        {
            bool pointFound = false;

            while (!pointFound)
            {
                Vector3 potentialNoise = new Vector3(CalculateNoiseValue(4.5f,10.0f), 0, CalculateNoiseValue(4.5f, 10.0f));
                float distance = Vector3.Distance(mPreviousPoint, mCurrentPoint + potentialNoise);
                if (distance > 16.0f)
                {
                    noiseValue = potentialNoise;
                    pointFound = true;
                }
            }

        }
       
        return noiseValue;
    }



    //will calculate a random float between two ranges
    float CalculateNoiseValue(float mMin, float mMax)
    {
        float Value = Random.Range(mMin, mMax);

        int multiplier = 0;
        while (multiplier == 0)
        {
            multiplier = Random.Range(-1, 2);
        }
        Value *= multiplier;


        return Value;
    }


    //random bomb placement co-routine
    IEnumerator PlaceBombWNoise(Vector3 mBombPosition, Vector3 mPreviousBombPosition)
    {
        GameObject currentBomb = Instantiate(Bomb, mBombPosition, Quaternion.identity);
        Vector3 currentPos = currentBomb.transform.position;
        currentPos += Noise(mPreviousBombPosition, mBombPosition);
        currentBomb.transform.parent = transform;
        currentPos.y = 0.1f;
        currentBomb.transform.position = currentPos;
        currentBomb.transform.localScale = new Vector3(0,1,0);
        float t = 0.0f;

        while (t < 1)
        {
            t += Time.deltaTime / scaleTimer;
            if (currentBomb)
            {
                currentBomb.transform.localScale = new Vector3(t, 1, t);
            }
            else
            {
                break;
            }
            yield return null;
        }

        if (currentBomb)
        {
            currentBomb.GetComponent<SCR_BossBomb>().StartBombTimer(timeToLand);
        }

    }


    //straight line strafe attack
    IEnumerator PlaceBomb(Vector3 mBombPosition)
    {
        GameObject currentBomb = Instantiate(Bomb, mBombPosition, Quaternion.identity);
        Vector3 currentPos = currentBomb.transform.position;
        currentBomb.transform.parent = transform;
        currentPos.y = 0.1f;
        currentBomb.transform.position = currentPos;
        currentBomb.transform.localScale = new Vector3(0, 1, 0);
        float t = 0.0f;

        while (t < 1)
        {
            t += Time.deltaTime / scaleTimer;
            if(currentBomb)
            {
                currentBomb.transform.localScale = new Vector3(t, 1, t);
            }
            else
            {
                break;
            }
            yield return null;
        }

        if (currentBomb)
        {
            currentBomb.GetComponent<SCR_BossBomb>().StartBombTimer(timeToLand);
        }

    }


    void DestroyCurrentBombs()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }




}
