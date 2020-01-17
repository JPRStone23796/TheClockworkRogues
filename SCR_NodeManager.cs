using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class SCR_NodeManager : MonoBehaviour
{
    [SerializeField]
    List<SCR_NodeClass> nodes;

    [HideInInspector]
    [SerializeField]
    List<SCR_NodeClass> finalMapPath = new List<SCR_NodeClass>();

    public float nodeRadius = 0.5f;
    public int AmountOfMapPathPoints;


    [SerializeField]
    private int amountOfLootRooms = 2;

    [SerializeField]
    private int amountOfChallangeRooms = 1;


    [SerializeField]
    List<SCR_NodeClass> CameraPoints = new List<SCR_NodeClass>();

    [SerializeField] private GameObject type;

    [SerializeField] private Material blockedRouteGateMaterial;

    //Initial Class creation
    public SCR_NodeManager()
    {
        nodes = new List<SCR_NodeClass>();
    }

    //return the final path created
    public List<SCR_NodeClass> ReturnFinalPathSelection()
    {
        return finalMapPath;
    }

    //Add a node to the current Map, requires a New node class
    public void AddNode(SCR_NodeClass newNode)
    {
        nodes.Add(newNode);
    }

    //return a node within the map structure, requires a node ID
   public SCR_NodeClass ReturnNode(STR_ID nodeID)
    {
        SCR_NodeClass node = new SCR_NodeClass();
        for(int i=0; i<nodes.Count;i++)
        {
            if(nodeID.Compare(nodes[i].returnID()))
            {
                node = nodes[i];
            }
        }
        return node;
    }

    //returns the amount of nodes within the map structure
    public int ReturnListSize()
    {
        return nodes.Count;
    }


    //return a node value at a specific index within the map structure
    public SCR_NodeClass ReturnNodeAtIndex(int index)
    {
        return nodes[index];
    }

    //return all nodes within the map structure
    public List<SCR_NodeClass> ReturnNodes()
    {
        return nodes;
    }



    //remove a node from the list, requires a node ID 
    public void RemoveNode(STR_ID nodeID)
    {
 
        for(int i=0;i<nodes.Count;i++)
        {
            SCR_NodeClass currentNode = nodes[i];
            List<STR_ID> currentNeighbours = currentNode.ReturnNeighbours();

            if(nodeID.Compare(currentNode.returnID()))
            {
                nodes.RemoveAt(i);
            }

            for(int j=0;j<currentNeighbours.Count;j++)
            {
                if (nodeID.Compare(currentNeighbours[j]))
                {
                    nodes[i].RemoveNeighbour(j);
                }
            }
        }
    }

    
    //Create a random path through the map structure
   public void CreatePath()
    {
        List<SCR_NodeClass> potentialMainPathCollection = new List<SCR_NodeClass>();
        List<SCR_NodeClass> sorteedMainPathRooms = new List<SCR_NodeClass>();
        List<SCR_NodeClass> keyRoomAreas = new List<SCR_NodeClass>();
        List<SCR_NodeClass> lootRooms = new List<SCR_NodeClass>();
        List<SCR_NodeClass> upgradeRooms = new List<SCR_NodeClass>();
        List<SCR_NodeClass> challangeRooms = new List<SCR_NodeClass>();
        finalMapPath = new List<SCR_NodeClass>();
        SCR_NodeClass startNode = new SCR_NodeClass(), endNode = new SCR_NodeClass();



        int numberOfPointsToBeSelected = AmountOfMapPathPoints;


        for (int i = 0; i < nodes.Count; i++)
        {
            SCR_NodeClass currentNode = nodes[i];
            if (currentNode.ReturnRoomType() == RoomType.PrimaryRoom || currentNode.ReturnRoomType() == RoomType.InitialFightRoom)
            {
                potentialMainPathCollection.Add(currentNode);
            }
            else if (currentNode.ReturnRoomType() == RoomType.KeyRoom)
            {
                keyRoomAreas.Add(currentNode);
                numberOfPointsToBeSelected--;
            }
            else if(currentNode.ReturnRoomType() == RoomType.StartRoom)
            {
                startNode = currentNode;
            }
            else if (currentNode.ReturnRoomType() == RoomType.EndRoom)
            {
                endNode = currentNode;
            }
            else if (currentNode.ReturnRoomType() == RoomType.LootRoom)
            {
                lootRooms.Add(currentNode);
            }
            else if (currentNode.ReturnRoomType() == RoomType.UpgradeRoom)
            {
                upgradeRooms.Add(currentNode);
            }
            else if (currentNode.ReturnRoomType() == RoomType.ChallangeRoom)
            {
                challangeRooms.Add(currentNode);
            }

        }






        potentialMainPathCollection.Insert(0, startNode);
        potentialMainPathCollection.Insert(potentialMainPathCollection.Count, endNode);

        finalMapPath.Add(potentialMainPathCollection[0]);
        for (int i = 0; i < keyRoomAreas.Count; i++)
        {
            finalMapPath.Add(keyRoomAreas[i]);
        }

        int pointsAdded = 0;

        bool startPointSelected = false;
        while (startPointSelected == false)
        {
            int rng = Random.Range(1, potentialMainPathCollection.Count - 1);

            if (potentialMainPathCollection[rng].ReturnRoomType() == RoomType.InitialFightRoom)
            {
                finalMapPath.Add(potentialMainPathCollection[rng]);
                pointsAdded++;
                startPointSelected = true;
            }
        }

        while (pointsAdded < numberOfPointsToBeSelected)
        {
            int rng = Random.Range(1, potentialMainPathCollection.Count - 1);

            if (potentialMainPathCollection[rng].ReturnRoomType() != RoomType.InitialFightRoom)
            {

                if (!finalMapPath.Contains(potentialMainPathCollection[rng]))
                    {                       
                        finalMapPath.Add(potentialMainPathCollection[rng]);
                        pointsAdded++;
                    }
            }

        }

        sorteedMainPathRooms.Add(finalMapPath[0]);
        for (int i = 1; i < finalMapPath.Count; i++)
        {
            Vector3 start = finalMapPath[i-1].returnID().body.transform.position;

            int currentPoint = 0;
            float distance = 1000000.0f;


                for (int j = 1; j < finalMapPath.Count; j++)
                {
                    Vector3 next = finalMapPath[j].returnID().body.transform.position;
                    if ((next - start).magnitude < distance)
                    {
                        if (!sorteedMainPathRooms.Contains(finalMapPath[j]))
                        {
                             currentPoint = j;
                            distance = (next - start).magnitude;
                         }
                       
                    }
                }

            SCR_NodeClass current = finalMapPath[currentPoint];
            sorteedMainPathRooms.Add(current);
            
        }


        finalMapPath = sorteedMainPathRooms;

        finalMapPath.Add(potentialMainPathCollection[potentialMainPathCollection.Count - 1]);



        SCR_PathFinder pathfinder = GetComponent<SCR_PathFinder>();
        pathfinder.finalPathPoints = finalMapPath;

        List<SCR_NodeClass> pathCalculated = new List<SCR_NodeClass>();

        pathCalculated.Add(finalMapPath[0]);
        for (int i = 0; i < finalMapPath.Count - 1; i++)
        {

            List<SCR_NodeClass> temp = pathfinder.FindingPath(finalMapPath[i], finalMapPath[i + 1], nodes);

            for (int j = 0; j < temp.Count; j++)
            {
                pathCalculated.Add(temp[j]);
            }
        }


        for (int i = 0; i<pathCalculated.Count; i++)
        {
            if (!finalMapPath.Contains(pathCalculated[i]))
            {
                finalMapPath.Add(pathCalculated[i]);
            }
        }


        ////CONNECT THE LOOT ROOMS


        
        List<SCR_NodeClass> selectedLootRooms = new List<SCR_NodeClass>();

        while (selectedLootRooms.Count < amountOfLootRooms)
        {
            int rng = Random.Range(0, lootRooms.Count - 1);

            if (!selectedLootRooms.Contains(lootRooms[rng]))
            {
                selectedLootRooms.Add(lootRooms[rng]);
            }
        }


      


        for (int i = 0; i < selectedLootRooms.Count; i++)
        {
            SCR_NodeClass closestNode = new SCR_NodeClass();
            float closestDistance = 1000000.0f;
            int currentNodeSelected = 0;

            Vector3 currentPoint = selectedLootRooms[i].returnID().body.transform.position;

            for (int j = 0; j < finalMapPath.Count; j++)
            {
                Vector3 currentNodesPosition = finalMapPath[j].returnID().body.transform.position;
                float nodeDistance = (currentNodesPosition - currentPoint).magnitude;

                if (nodeDistance < closestDistance)
                {
                    closestDistance = nodeDistance;
                    currentNodeSelected = j;
                }
            }


            List<SCR_NodeClass> lootPathCalculated = new List<SCR_NodeClass>();

            lootPathCalculated = pathfinder.FindingPath(selectedLootRooms[i], finalMapPath[currentNodeSelected], nodes);

            for (int z = 0; z < lootPathCalculated.Count; z++)
            {
                
                    finalMapPath.Add(lootPathCalculated[z]);
            }
            finalMapPath.Add(selectedLootRooms[i]);
            if (selectedLootRooms[i].specialRoomTriggers != null)
            {
                SCR_SpecialRoomTriggers triggerScript = selectedLootRooms[i].specialRoomTriggers;
                StartCoroutine(triggerScript.CloseGateway(0.1f, false));
            }
          

        }


        ////CONNECT THE UPGRADE ROOMS


        





        for (int i = 0; i < upgradeRooms.Count; i++)
        {
            SCR_NodeClass closestNode = new SCR_NodeClass();
            float closestDistance = 1000000.0f;
            int currentNodeSelected = 0;

            Vector3 currentPoint = upgradeRooms[i].returnID().body.transform.position;

            for (int j = 0; j < finalMapPath.Count; j++)
            {
                Vector3 currentNodesPosition = finalMapPath[j].returnID().body.transform.position;
                float nodeDistance = (currentNodesPosition - currentPoint).magnitude;

                if (nodeDistance < closestDistance)
                {
                    closestDistance = nodeDistance;
                    currentNodeSelected = j;
                }
            }


            List<SCR_NodeClass> upgradePathCalculated = new List<SCR_NodeClass>();

            upgradePathCalculated = pathfinder.FindingPath(upgradeRooms[i], finalMapPath[currentNodeSelected], nodes);

            for (int z = 0; z < upgradePathCalculated.Count; z++)
            {

                finalMapPath.Add(upgradePathCalculated[z]);
            }
            finalMapPath.Add(upgradeRooms[i]);

            if (upgradeRooms[i].specialRoomTriggers != null)
            {
                SCR_SpecialRoomTriggers triggerScript = upgradeRooms[i].specialRoomTriggers;
                StartCoroutine(triggerScript.CloseGateway(0.1f, false));
            }

        }


        ///select the challange rooms
        ///

        List<SCR_NodeClass> selectedChallangeRooms = new List<SCR_NodeClass>();

        while (selectedChallangeRooms.Count < amountOfChallangeRooms)
        {
            int rng = Random.Range(0, challangeRooms.Count - 1);

            if (!selectedChallangeRooms.Contains(challangeRooms[rng]))
            {
                selectedChallangeRooms.Add(challangeRooms[rng]);
            }
        }





        for (int i = 0; i < selectedChallangeRooms.Count; i++)
        {
            SCR_NodeClass closestNode = new SCR_NodeClass();
            float closestDistance = 1000000.0f;
            int currentNodeSelected = 0;

            Vector3 currentPoint = selectedChallangeRooms[i].returnID().body.transform.position;

            for (int j = 0; j < finalMapPath.Count; j++)
            {
                Vector3 currentNodesPosition = finalMapPath[j].returnID().body.transform.position;
                float nodeDistance = (currentNodesPosition - currentPoint).magnitude;

                if (nodeDistance < closestDistance)
                {
                    closestDistance = nodeDistance;
                    currentNodeSelected = j;
                }
            }


            List<SCR_NodeClass> challangePathCalculated = new List<SCR_NodeClass>();

            challangePathCalculated = pathfinder.FindingPath(selectedChallangeRooms[i], finalMapPath[currentNodeSelected], nodes);

            for (int z = 0; z < challangePathCalculated.Count; z++)
            {

                finalMapPath.Add(challangePathCalculated[z]);
            }
            finalMapPath.Add(selectedChallangeRooms[i]);


        }


        ///FIND A CAMERA PATH WAY

        CameraPoints = new List<SCR_NodeClass>();
        CameraPoints = pathfinder.FindingPath(startNode, endNode, finalMapPath);
        CameraPoints.Add(startNode);


        ///set a wall

        for (int i = 0; i < nodes.Count; i++)
        {
            SCR_NodeClass currentNode = nodes[i];
            currentNode.closeEntryPoints(false);

            currentNode.checkObject(type);

        }


        for (int i = 0; i < nodes.Count; i++)
        {
            SCR_NodeClass currentNode = nodes[i];

            if (currentNode.roomManager != null)
            {
                if (!finalMapPath.Contains(currentNode))
                {
                    currentNode.closeEntryPoints(true);
                    currentNode.roomManager.CloseWalls();
                }
                else
                {
                    currentNode.closeEntryPoints(true);
                    currentNode.roomManager.OpenWalls();
                    currentNode.roomManager.SpawnRoom();
                }
            }
            else
            {

                if (!finalMapPath.Contains(currentNode))
                {
                    currentNode.closeEntryPoints(true);
                }
                else
                {
                    if (currentNode.specialRoomTriggers != null)
                    {
                        StartCoroutine(currentNode.specialRoomTriggers.CloseGateway(1.0f, false));
                    }
                }
            }
        }


      
        




        //// set the emission of lamp bois


        for (int i = 0; i < nodes.Count; i++)
        {
            SCR_NodeClass currentNode = nodes[i];

                if (!finalMapPath.Contains(currentNode))
                {
                    List<GameObject> currentLamposts = currentNode.ReturnLocalLamposts();
                    
                    for (int j = 0; j < currentLamposts.Count; j++)
                    {
                        if (currentLamposts[j].GetComponent<SCR_LevelMaterials>())
                        {
                            SCR_LevelMaterials current = currentLamposts[j].GetComponent<SCR_LevelMaterials>();
                            current.setObjectNonEmissive();
                        }                                              
                    }
                    currentLamposts.Clear();

                    List<GameObject> currentBuildings = currentNode.ReturnLocalBuildings();
                    for (int j = 0; j < currentBuildings.Count; j++)
                    {
                        if (currentBuildings[j].GetComponent<SCR_LevelMaterials>())
                        {
                            SCR_LevelMaterials current = currentBuildings[j].GetComponent<SCR_LevelMaterials>();
                            current.setObjectNonEmissive();
                        }
                    }
                    currentBuildings.Clear();
            }
              
        }




        //change the material of the gates


        if (blockedRouteGateMaterial)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                SCR_NodeClass currentNode = nodes[i];

                if (!finalMapPath.Contains(currentNode))
                {
                    List<GameObject> currentGates = currentNode.ReturnNodeGates();

                    for (int j = 0; j < currentGates.Count; j++)
                    {
                        if (currentGates[j])
                        {


                            for (int z = 0; z < currentGates[j].transform.childCount; z++)
                            {
                                if (currentGates[j].transform.GetChild(z).GetComponent<MeshRenderer>() != null)
                                {
                                    MeshRenderer currentGatesRenderer = currentGates[j].transform.GetChild(z).GetComponent<MeshRenderer>();


                                    Material[] currentGateMaterials = new Material[currentGatesRenderer.materials.Length];
                                    for (int x = 0; x < currentGateMaterials.Length; x++)
                                    {
                                        currentGateMaterials[x] = blockedRouteGateMaterial;
                                    }
                                    currentGatesRenderer.materials = currentGateMaterials;

                                }
                            }


                          

                        }
                    }
                }
            }
        }





    }


    public List<Vector3> ReturnCameraPath()
    {   
        
        List<Vector3> temp = new List<Vector3>();

        for (int i = 0; i < CameraPoints.Count; i++)
        {
            temp.Add(CameraPoints[i].returnID().body.transform.position);
        }
        return temp;
    }


    public List<SCR_NodeClass> ReturnCameraNodes()
    {
        return CameraPoints;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(SCR_NodeManager))]
public class NodeManager : Editor
{
    SCR_NodeManager nodeManager;
    List<SCR_NodeClass> finalPathSelection = new List<SCR_NodeClass>();

    //retreives necessary values when clicked on
    private void OnEnable()
    {
        nodeManager = (SCR_NodeManager)target;
 
    }

    //draws handles within the map
    private void OnSceneGUI()
    {

        for (int i = 0; i < nodeManager.ReturnListSize(); i++)
        {
            finalPathSelection = nodeManager.ReturnFinalPathSelection();
            SCR_NodeClass currentNode = nodeManager.ReturnNodeAtIndex(i);

            List<STR_ID> currentNeighbours = currentNode.ReturnNeighbours();
            Transform nodePos = currentNode.returnID().body.transform;
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            Handles.Label(nodePos.position, nodePos.gameObject.name, style);
            Color currentColour = Color.blue;

            RoomType currentRoomType = currentNode.ReturnRoomType();
            if(currentRoomType == RoomType.PrimaryRoom || currentRoomType == RoomType.InitialFightRoom || currentRoomType == RoomType.KeyRoom)
            {
                if (finalPathSelection.Contains(currentNode))
                {
                    currentColour = Color.green;
                }
                else
                {
                    currentColour = new Color(1.0f,0.64f,0.0f,1.0f);
                }
               
            }
            else if (currentRoomType == RoomType.SecondaryPathway)
            {
                currentColour = Color.gray;
            }
            else if(currentRoomType == RoomType.StartRoom || currentRoomType == RoomType.EndRoom)
            {
                currentColour = new Color(1.0f, 0.64f, 0.0f, 1.0f);
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