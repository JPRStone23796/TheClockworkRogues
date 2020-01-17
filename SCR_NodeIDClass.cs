using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[ExecuteInEditMode]
[System.Serializable]
//A node ID class , stored only on the actual node objects in order to identify themselves within the node manager
public class SCR_NodeIDClass:MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    STR_ID currentNodeID;

    
    public SCR_NodeIDClass()
    {
        currentNodeID = new STR_ID();
    }

 

    public void SetBody(GameObject bodyObject)
    {
        currentNodeID.body = bodyObject;
    }

    public STR_ID RetreiveID()
    {
        return currentNodeID;
    }
  
  


}
#if UNITY_EDITOR
//Draws all Handles
[CustomEditor(typeof(SCR_NodeIDClass))]
public class nodeIcon : Editor
{
    SCR_NodeManager nodes;
    private void OnEnable()
    {
        nodes = GameObject.FindGameObjectWithTag("PathFinderManager").GetComponent<SCR_NodeManager>();
       
    }


    private void OnSceneGUI()
    {
        for (int i = 0; i < nodes.ReturnListSize(); i++)
        {
            SCR_NodeClass currentNode = nodes.ReturnNodeAtIndex(i);

            List<STR_ID> currentNeighbours = currentNode.ReturnNeighbours();
            Transform nodePos = currentNode.returnID().body.transform;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            Handles.Label(nodePos.position, nodePos.gameObject.name, style);
            Color currentColour = Color.blue;
            RoomType currentRoomType = currentNode.ReturnRoomType();
            if (currentRoomType == RoomType.PrimaryRoom || currentRoomType == RoomType.InitialFightRoom || currentRoomType == RoomType.KeyRoom)
            {
                    currentColour = new Color(1.0f, 0.64f, 0.0f, 1.0f);
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
            Handles.DrawSolidDisc(nodePos.position, Vector3.up, nodes.nodeRadius);


            Handles.color = Color.blue;
            for (int j = 0; j < currentNeighbours.Count; j++)
            {
                Transform neighbourPos = currentNeighbours[j].body.transform;
                Handles.DrawLine(nodePos.position, neighbourPos.position);
            }

        }
    }





}
#endif