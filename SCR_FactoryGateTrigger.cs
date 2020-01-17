using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_FactoryGateTrigger : MonoBehaviour, IFightRoomTrigger
{


    private Vector3 startPosition, endPosition;
    [SerializeField]
    private STR_Gate primaryGate;

    [SerializeField]
    GameObject firstFactoryGate, secondFactoryGate;
    private IRoom roomManager;
    private bool player1Within = false, player2Within = false;

    void Awake()
    {
        Transform current = primaryGate.gateBody.transform;
        float size = primaryGate.gateBody.GetComponent<BoxCollider>().bounds.size.y;
        primaryGate.returnEnd = current.position;
        primaryGate.returnStart = new Vector3(current.position.x, current.position.y - size, current.position.z);
    }



    public void ClosePrimary(bool Close, float time)
    {
        StartCoroutine(MoveGate(Close, time));
    }

    public void CloseSecondary(bool Close, float time)
    {
        StartCoroutine(OpenFactoryGates(Close,time));

    }

    public void setRoomManager(GameObject managerObject)
    {
        roomManager = managerObject.GetComponent<IRoom>();
    }


    bool PlayerAlive(string mTag)
    {
        float health;
        GameObject playerChecked = null;
        switch (mTag)
        {
            case "Player1":
                playerChecked = GameObject.FindGameObjectWithTag("Player1");
                break;
            case "Player2":
                playerChecked = GameObject.FindGameObjectWithTag("Player2");
                break;
            default:
                playerChecked = null;
                break;
        }
        if (playerChecked != null)
        {
            health = playerChecked.GetComponent<SCR_PlayerHealth>().player.GetHealth();

            if (health > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    void FixedUpdate()
    {
        if (player1Within && !PlayerAlive("Player2"))
        {
            roomManager.BeginLevel();
        }
        else if (player2Within && !PlayerAlive("Player1"))
        {
            roomManager.BeginLevel();
        }
        else if (player1Within && player2Within && roomManager != null)
        {
            roomManager.BeginLevel();
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player1"))
        {
            player1Within = true;
        }

        if (collision.gameObject.CompareTag("Player2"))
        {
            player2Within = true;
        }

    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player1"))
        {
            player1Within = false;
        }

        if (collision.gameObject.CompareTag("Player2"))
        {
            player2Within = false;
        }
    }

    IEnumerator MoveGate(bool levelStarted, float moveTimer)
    {
        STR_Gate current = primaryGate;
        GameObject currentGateBody = current.gateBody;
        float spawnProgress = 0;
        Vector3 currentPos = Vector3.zero;
        Vector3 endPos = Vector3.zero;
        switch (levelStarted)
        {
            case true: currentPos = current.returnEnd; endPos = current.returnStart; break;
            case false: currentPos = current.returnStart; endPos = current.returnEnd; break;
        }

        while (spawnProgress < 1)
        {
            spawnProgress += Time.deltaTime / moveTimer;
            currentGateBody.transform.position = Vector3.Lerp(currentPos, endPos, spawnProgress);
            yield return null;
        }
    }


    IEnumerator OpenFactoryGates(bool close, float timer)
    {

        float startAngle = 0.0f, endAngle = 90.0f;
        if(!close)
        {
            startAngle = 90.0f;
            endAngle = 0.0f;
        }



        float t = 0.0f;
        while (t<1.0f)
        {
            t += Time.deltaTime / timer;



            float anglePerSecond = Mathf.Lerp(startAngle, endAngle, t);
            Vector3 current = secondFactoryGate.transform.localEulerAngles;
            firstFactoryGate.transform.localEulerAngles = new Vector3(current.x, anglePerSecond*-1, current.z);

            current = secondFactoryGate.transform.localEulerAngles;
            secondFactoryGate.transform.localEulerAngles = new Vector3(current.x, anglePerSecond, current.z);
            yield return null;
        }

    }




}
