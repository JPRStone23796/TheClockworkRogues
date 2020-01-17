using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class SCR_CogMovement : MonoBehaviour
{

	[SerializeField] Vector3 _cogDirection = Vector3.zero;



    [Header("Variables affect only x and z speeds")]
    [SerializeField] private float cogMovementSpeed;
    [SerializeField] private float speedDampening;

    [Header("Variables affect the height speeds")]
    [SerializeField] private float gravity;
    [SerializeField] private float jumpHeight;


    [Header("Affects the initial speed before the cog beings its decline")]
    [SerializeField] private float initialSpeedMultiplier;
    [Header("Affects the value of gravity while the cog is falling down")]
    [SerializeField] private float gravityIncreaseMultiplier;
    [Header("Value of when the cog will stop bouncing")]
    [SerializeField] private float mininimumJumpValue;
    [Header("affects the height the ball will bounce to when the ground is hit")]
    [SerializeField] private float bounceMultiplier;




    [Header("Variables for when the cog is moving")]
    [SerializeField] private float chaseMovementSpeed;
    [SerializeField] private float cogRotationSpeed;
    [SerializeField] private float playerRange;
    [SerializeField] private float rotationSpeed;


    [Header("Floating Up & Down")]
    [SerializeField] float targetDistance = .01f;
    float targetYWorldPos;

    [SerializeField] float floatSpeed = 0.001f;
    float currentFloatVelocity;

    [Header("Rotation")]
    [SerializeField] float rotateSpeed = 35;


    private float startHeight = 0;
    private float initialVerticalAcceleration = 0;
    private bool initialUpwards = true;
    private bool _bouncing = true, chasingTarget = false;

    private bool bCanChase = false;
    private float timer;


    private GameObject _player1, _player2 , _currentPlayer;

    float _currentTimer;

    private GameObject tutorialManager;

    void Awake()
    {
        float rng = Random.Range(0.8f, 1.2f);
        cogMovementSpeed /= rng;
        speedDampening /= rng;
        jumpHeight /= rng;
        chaseMovementSpeed /= rng;
        _player1 = GameObject.FindGameObjectWithTag("Player1");
        _player2 = GameObject.FindGameObjectWithTag("Player2");
        tutorialManager = GameObject.FindGameObjectWithTag("TutorialManager");
        timer = 0.0f;

        targetYWorldPos = (transform.position + (Vector3.up * targetDistance)).y;
    }


    
    void Update()
    {

        if (_bouncing)
        {
            CogBouncing();
        }

        if (!_bouncing || bCanChase)
        {
            if (chasingTarget)
            {

                if (_currentPlayer.activeInHierarchy)
                {
                    ChasePlayer();
                }
                else
                {
                    CheckPlayers();
                }
            }
            else
            { // Not chasing players (not moving)
                CheckPlayerDistances();

                if (transform.position.y < targetYWorldPos)
                    currentFloatVelocity += floatSpeed * Time.deltaTime;
                else
                    currentFloatVelocity -= floatSpeed * Time.deltaTime;

                transform.position += Vector3.up * currentFloatVelocity;

                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
            }
        }
       
    }



    void CheckPlayers()
    {
        float currentLowestDistance = 10000.0f;
        GameObject selected = null;

        if (_player1)
        {
            if (_player1.activeInHierarchy)
            {
                float distance = Vector3.Distance(_player1.transform.position, transform.position);

                if (distance < currentLowestDistance)
                {
                    currentLowestDistance = distance;
                    selected = _player1;
                }
            }
        }


        if (_player2)
        {
            if (_player2.activeInHierarchy)
            {
                float distance = Vector3.Distance(_player2.transform.position, transform.position);

                if (distance < currentLowestDistance)
                {
                    currentLowestDistance = distance;
                    selected = _player2;
                }
            }
        }


        if (selected)
        {           
                _currentPlayer = selected;
        }
        else
        {
           transform.Rotate(0, rotationSpeed*Time.deltaTime, 0);
           chasingTarget = false;
        }

    }



    void ChasePlayer()
    {
        Vector3 _direction = _currentPlayer.transform.position - transform.position;
        transform.position += (transform.forward * chaseMovementSpeed) * Time.deltaTime;
        Quaternion currentRot = transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(_direction);
        transform.rotation = Quaternion.Lerp(currentRot, targetRot, Time.deltaTime * cogRotationSpeed);
        cogRotationSpeed += Time.deltaTime * 0.7f;


        if (Vector3.Distance(_currentPlayer.transform.position, transform.position) < 0.4f)
        {
            if (tutorialManager)
            {
                SCR_GameManager.instance.CogCollected(5);
            }
            else
            {
                SCR_GameManager.instance.CogCollected();
               
            }

            SCR_AudioManager.instance.PlayCogCollectedClip();
            Destroy(gameObject);
        }



        if (Vector3.Distance(_currentPlayer.transform.position, transform.position) < 15)
        {
            cogRotationSpeed += Time.deltaTime;
        }
        chaseMovementSpeed += Time.deltaTime;
    }


    void CheckPlayerDistances()
    {
        float currentLowestDistance = 10000.0f;
        GameObject selected = null;

        if (_player1)
        {
            if (_player1.activeInHierarchy)
            {
                float distance = Vector3.Distance(_player1.transform.position, transform.position);

                if (distance < currentLowestDistance)
                {
                    currentLowestDistance = distance;
                    selected = _player1;
                }
            }
        }


        if (_player2)
        {
            if (_player2.activeInHierarchy)
            {
                float distance = Vector3.Distance(_player2.transform.position, transform.position);

                if (distance < currentLowestDistance)
                {
                    currentLowestDistance = distance;
                    selected = _player2;
                }
            }
        }



        if (selected)
        {
            if (currentLowestDistance <= playerRange)
            {
                _currentPlayer = selected;
                chasingTarget = true;
                transform.LookAt(_currentPlayer.transform);
            }
        }

    }

    void CogBouncing()
    {
        timer += Time.deltaTime;
        float _speedMultiplier = 1;
        if (initialUpwards)
        {
            _speedMultiplier *= initialSpeedMultiplier;
        }

        Vector3 speedVector = _cogDirection;

        speedVector.x *= cogMovementSpeed;
        speedVector.z *= cogMovementSpeed;
        speedVector.y *= jumpHeight;
        speedVector.y *= _speedMultiplier;

        transform.position += speedVector * Time.deltaTime;

        if (timer >= 1.5f)
        {
            bCanChase = true;
        }

        float gravityValue = gravity;
        if (jumpHeight < 0)
        {
            gravityValue = gravity * gravityIncreaseMultiplier;
        }

        if (jumpHeight >= 0 && jumpHeight <= 0.1)
        {
            gravityValue = gravityValue * 0.2f;
        }
        jumpHeight -= gravityValue * Time.deltaTime;


        if (transform.position.y <= startHeight)
        {
            jumpHeight = initialVerticalAcceleration * bounceMultiplier;
            initialVerticalAcceleration = jumpHeight;

            if (initialVerticalAcceleration <= mininimumJumpValue)
            {
                _bouncing = false;
            }
        }

        if (_cogDirection.y <= 0 && initialUpwards)
        {
            initialUpwards = false;
        }
        cogMovementSpeed = Mathf.Clamp(cogMovementSpeed - (speedDampening * Time.deltaTime), 0, 50);
    }

    public void SetSpeed(float movementSpeed, float movementDamp = 1)
    {
        cogMovementSpeed = movementSpeed;
        speedDampening = movementDamp;
    }

   public void SetDirection(Vector3 mCogDirection)
    {
        _cogDirection = mCogDirection;
        startHeight = transform.position.y;
        initialVerticalAcceleration = jumpHeight;

    }




  


}
