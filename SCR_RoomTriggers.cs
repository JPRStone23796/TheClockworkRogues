using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_RoomTriggers : MonoBehaviour, IFightRoomTrigger
{
    private Vector3 startPosition, endPosition;
    [SerializeField]
    private STR_Gate primaryGate, secondaryGate;
    private IRoom roomManager;
    private bool player1Within = false, player2Within = false;

    void Awake()
    {
        Transform current = primaryGate.gateBody.transform;
        float size = primaryGate.gateBody.GetComponent<BoxCollider>().bounds.size.y;
        primaryGate.returnEnd = current.position;
        primaryGate.returnStart = new Vector3(current.position.x, current.position.y - size, current.position.z);

        size = secondaryGate.gateBody.GetComponent<BoxCollider>().bounds.size.y;
        current = secondaryGate.gateBody.transform;
        secondaryGate.returnEnd = current.position;
        secondaryGate.returnStart = new Vector3(current.position.x, current.position.y - size, current.position.z);
       
    }



    public void ClosePrimary(bool Close, float time)
    {
        StartCoroutine(MoveGate(Close, time, true));
    }

    public void CloseSecondary(bool Close, float time)
    {
        StartCoroutine(MoveGate(Close, time, false));
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
            default: playerChecked = null;
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
        else if(player2Within && !PlayerAlive("Player1"))
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

    IEnumerator MoveGate(bool levelStarted, float moveTimer, bool primary)
    {
        STR_Gate current = primaryGate;
        if (!primary)
        {
            current = secondaryGate;
        }

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


}

[System.Serializable]
struct STR_Gate
{
    public GameObject gateBody;
    private Vector3 startPos, endPos;

    public Vector3 returnStart
    {
        get { return startPos; }
        set { startPos = value; }
    }


    public Vector3 returnEnd
    {
        get { return endPos; }
        set { endPos = value; }
    }
}