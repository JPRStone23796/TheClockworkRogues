using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum RoomType
{
    [SerializeField]
    PrimaryRoom, SecondaryPathway,StartRoom, EndRoom,KeyRoom, LootRoom, UpgradeRoom, BlockedRoute, ChallangeRoom, InitialFightRoom, smoothingPath
};



[System.Serializable]
public class SCR_NodeClass
{
    [SerializeField]
    STR_ID nodeID;
    [SerializeField]
    List<STR_ID> nodeNeigbours;
    [SerializeField]
    RoomType roomType;


    [Header("Nearest Buildings")]
    [SerializeField]
    List<GameObject> localBuildings;

    [Header("Nearest Lamp Posts")]
    [SerializeField]
    List<GameObject> localLampPosts;

    [Header("Local entry Points")]
    [SerializeField]
    List<GameObject> entryPoints;


    [SerializeField]
    public SCR_RoomManager roomManager;

    [SerializeField] public SCR_SpecialRoomTriggers specialRoomTriggers;

    //set up class
    public SCR_NodeClass()
    {
        nodeID = new STR_ID();
        nodeNeigbours = new List<STR_ID>();
        roomType = RoomType.SecondaryPathway;
    }

    //set a nodes room type
    public void SetType(RoomType Type)
    {
        roomType = Type;
    }

    //set a nodes ID
    public void SetID(STR_ID newID)
    {
        nodeID = newID;
    }

    //add a neighbouring node to the current node, requires the neighbours ID
    public void AddNeighbour(STR_ID neighbourID)
    {
        bool InList = false;
        for(int i=0;i< nodeNeigbours.Count;i++)
        {
            if(neighbourID.Compare(nodeNeigbours[i]))
            {
                InList = true;
            }
        }

        if(InList==false)
        {
            nodeNeigbours.Add(neighbourID);
        }
    }


    //remove a neighbour from the current node, requires the neighbours ID
    public void RemoveNeighbour(STR_ID neighbourID)
    {
        bool InList = false;
        int position = 0;
        for (int i = 0; i < nodeNeigbours.Count; i++)
        {
            if (neighbourID.Compare(nodeNeigbours[i]))
            {
                InList = true;
                position = i;
            }
        }

        if (InList == true)
        {
            nodeNeigbours.RemoveAt(position);
        }
    }

    //remove a neighbour from the current node, requires the neighbours index
    public void RemoveNeighbour(int index)
    {
        nodeNeigbours.RemoveAt(index);
    }


    public bool checkObject(GameObject wall)
    {

        bool isIn = false;
        for (int i = 0; i < entryPoints.Count; i++)
        {
            if (entryPoints[i] == wall)
            {
                //Debug.Log(nodeID.body.name);
                //isIn = true;
            }
        }

        return isIn;
    }




    ///return functions

    public List<STR_ID> ReturnNeighbours()
    {
        return nodeNeigbours;
    }

    public STR_ID returnID()
    {
        return nodeID;
    }

    public RoomType ReturnRoomType()
    {
        return roomType;
    }

    public List<GameObject> ReturnLocalLamposts()
    {
        return localLampPosts;
    }
    public List<GameObject> ReturnLocalBuildings()
    {
        return localBuildings;
    }



    public List<GameObject> ReturnNodeGates()
    {
        return entryPoints;
    }


    public void closeEntryPoints(bool close)
    {
        if (entryPoints.Count > 0)
        {
            for (int i = 0; i < entryPoints.Count; i++)
            {
                if (entryPoints[i] != null)
                {
                    entryPoints[i].SetActive(close);
                }
            }
        }
    }


}


//struct which stores a nodes primary key
[System.Serializable]
public struct STR_ID
{
    [SerializeField]
    public GameObject body;

    public bool Compare(STR_ID nodeID)
    {
        bool Comparison = false;
        if(nodeID.body== body)
        {
            Comparison = true;
        }

        return Comparison;

    }
}