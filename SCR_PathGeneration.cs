using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SCR_PathGeneration : MonoBehaviour
{
    List<Vector3> cameraPath = new List<Vector3>();

    [SerializeField]
    GameObject Map;

    float offset;
    SCR_NodeManager nodeManager;
    private void Start()
    {
        nodeManager = GameObject.FindGameObjectWithTag("PathFinderManager").GetComponent<SCR_NodeManager>();
        offset = Map.transform.position.y +(transform.position.y - Map.transform.position.y);
    }

    // Update is called once per frame
    void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Space))
        {
            nodeManager.CreatePath();
            cameraPath = nodeManager.ReturnCameraPath();
            cameraSpeed = speed;
            transform.position = cameraPath[0];
            StartCoroutine(Switch());
        }
	}
  
    public float speed = 1.0F;
    private float cameraSpeed;

    public float CameraAcceleration = 2.0f;
    private float startTime;
    private float journeyLength;
    IEnumerator Switch()
    {
        int i = 0;
        while(i<cameraPath.Count-1)
        {
            Vector3 start = cameraPath[i];
            start = new Vector3(start.x, offset, start.z);
            Vector3 next = cameraPath[i+1];
            next = new Vector3(next.x, offset, next.z);
            startTime = Time.time;
            journeyLength = Vector3.Distance(start, next);
            while ((transform.position-next).magnitude>0.4)
            {
                float distCovered = (Time.time - startTime) * cameraSpeed;
                float fracJourney = distCovered / journeyLength;
                transform.position = Vector3.Lerp(start, next, fracJourney);
                cameraSpeed += Time.deltaTime * CameraAcceleration;
                
                yield return null;
            }
            i++;
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(SCR_PathGeneration))]
public class CameraPathSCR : Editor
{
    SCR_NodeManager nodeManager;
    List<SCR_NodeClass> finalPathSelection = new List<SCR_NodeClass>();

    //retreives necessary values when clicked on
    private void OnEnable()
    {
        nodeManager = GameObject.FindGameObjectWithTag("PathFinderManager").GetComponent<SCR_NodeManager>();

    }

    //draws handles within the map
    private void OnSceneGUI()
    {

        for (int i = 0; i < nodeManager.ReturnListSize(); i++)
        {
            finalPathSelection = nodeManager.ReturnCameraNodes();
            SCR_NodeClass currentNode = nodeManager.ReturnNodeAtIndex(i);

            List<STR_ID> currentNeighbours = currentNode.ReturnNeighbours();
            Transform nodePos = currentNode.returnID().body.transform;
            Handles.Label(nodePos.position, nodePos.gameObject.name);
            Color currentColour = Color.blue;

            RoomType currentRoomType = currentNode.ReturnRoomType();
            if (currentRoomType == RoomType.PrimaryRoom || currentRoomType == RoomType.InitialFightRoom || currentRoomType == RoomType.KeyRoom)
            {
                if (finalPathSelection.Contains(currentNode))
                {
                    currentColour = Color.green;
                }
                else
                {
                    currentColour = Color.black;
                }

            }
            else if (currentRoomType == RoomType.SecondaryPathway)
            {
                currentColour = Color.gray;
            }
            else if (currentRoomType == RoomType.StartRoom || currentRoomType == RoomType.EndRoom)
            {
                currentColour = Color.black;
            }
            else if (currentRoomType == RoomType.LootRoom)
            {
                currentColour = Color.blue;
            }
            else if (currentRoomType == RoomType.BlockedRoute)
            {
                currentColour = Color.red;
            }
            else if (currentRoomType == RoomType.UpgradeRoom)
            {
                currentColour = Color.yellow;
            }
            else if (currentRoomType == RoomType.ChallangeRoom)
            {
                currentColour = Color.magenta;
            }
            else if (currentRoomType == RoomType.smoothingPath)
            {
                currentColour = Color.white;
            }
            Handles.color = currentColour;
            Handles.DrawSolidDisc(nodePos.position, Vector3.up, nodeManager.nodeRadius);



            for (int j = 0; j < currentNeighbours.Count; j++)
            {
                Transform neighbourPos = currentNeighbours[j].body.transform;
                currentColour = Color.blue;
                SCR_NodeClass NeighbourNode = nodeManager.ReturnNode(currentNeighbours[j]);
                if (finalPathSelection.Contains(NeighbourNode) && finalPathSelection.Contains(currentNode))
                {
                    currentColour = Color.green;
                }
                Handles.color = currentColour;
                Handles.DrawLine(nodePos.position, neighbourPos.position);
            }

        }
    }


}
#endif