using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PathFinder : MonoBehaviour
{


    SCR_NodeClass goal, start;
    SCR_NodeClass currentNode;
    List<CalculatePath> open = new List<CalculatePath>();
    List<CalculatePath> closed = new List<CalculatePath>();
    Queue<SCR_NodeClass> finalPathQueue = new Queue<SCR_NodeClass>();
    int currentPos;
    List<SCR_NodeClass> finalMapPathway;
    public bool pathfound;
    int pathEnd;
    SCR_NodeManager nodeManager;



    List<SCR_NodeClass> listOfNodes = new List<SCR_NodeClass>();


    public List<SCR_NodeClass> finalPathPoints = new List<SCR_NodeClass>();

    //Main Pathfinding function, will be called to generate the path and will return the final path
    public List<SCR_NodeClass> FindingPath(SCR_NodeClass startPoint, SCR_NodeClass endPoint, List<SCR_NodeClass> nodeSelection)
    {
        listOfNodes = new List<SCR_NodeClass>();
        listOfNodes = nodeSelection;
        pathfound = false;
        finalMapPathway = new List<SCR_NodeClass>();
        open = new List<CalculatePath>();
        closed = new List<CalculatePath>();
        nodeManager = GetComponent<SCR_NodeManager>();
        start = startPoint;
        goal = endPoint;
        open.Add(new CalculatePath(startPoint, null));
        GeneratePath();
        FindPath();

        open.Clear();
        closed.Clear();
        return finalMapPathway;
    }

    //Will sort a path through the map structure until the goal is reached
    void GeneratePath()
    {
        while (pathfound == false)
        {
            CalculatePoint();
            CalculateNeighbours();
        }
    }

    //Will select the next point to review within the map
    void CalculatePoint()
    {
        float Fcost = 10000000.0f;
        Vector3 startPos = start.returnID().body.transform.position;
        Vector3 endPos= goal.returnID().body.transform.position;
        for (int i = 0; i < open.Count; i++)
        {
            STR_ID currentID = open[i].ReturningMainNode().returnID();
        
            var CurrentFCost = CalculateCost(currentID.body.transform.position, startPos) + CalculateCost(currentID.body.transform.position, endPos);
            if (CurrentFCost < Fcost)
            {
                Fcost = CurrentFCost;
                currentNode = open[i].ReturningMainNode();
                currentPos = i;
            }
        }
    }

    //Will calculate the F cost based on map positions
    float CalculateCost(Vector2 currentNodePosition, Vector2 StartPosition)
    {
        var x = Mathf.Abs(currentNodePosition.x - StartPosition.x);
        var y = Mathf.Abs(currentNodePosition.y - StartPosition.y);
        return x + y;
    }


    //will add any potential neighbouring nodes to the potential path list if they haven't already, will check to see if the path has reached the goal
    void CalculateNeighbours()
    {

        List<STR_ID> neighbours = currentNode.ReturnNeighbours();

        for(int i=0;i<neighbours.Count;i++)
        {
            SCR_NodeClass neighbourNode = nodeManager.ReturnNode(neighbours[i]);
            STR_ID currentNeighbourID = neighbourNode.returnID();
            STR_ID currentNodeID = currentNode.returnID();
            if(currentNodeID.Compare(currentNeighbourID)==false)
            {
             
                if (listOfNodes.Contains(neighbourNode))
                {
                    if (neighbourNode.ReturnRoomType() != RoomType.BlockedRoute)
                    {
                        var current = new CalculatePath(neighbourNode, currentNode);

                        if (neighbourNode.ReturnRoomType() == RoomType.InitialFightRoom || neighbourNode.ReturnRoomType() == RoomType.ChallangeRoom)
                        {
                            if (!finalPathPoints.Contains(neighbourNode))
                            {
                                closed.Add(current);
                            }
                        }


                        
                        if ((!open.Contains(current)) && (!closed.Contains(current)))
                        {
                            if (CheckOpenList(currentNodeID, currentNeighbourID) == false && CheckClosedList(currentNodeID, currentNeighbourID) == false)
                            {
                                open.Add(current);
                            }

                        }
                    }
                }

            }

        }

        closed.Add(open[currentPos]);

        Vector3 endPos = goal.returnID().body.transform.position;
        STR_ID currentID = currentNode.returnID();
        Vector3 currentPosition = currentNode.returnID().body.transform.position;
        
        if (currentNode == goal)
        {          
                pathfound = true;
                pathEnd = closed.Count - 1;           
        }
        open.Remove(open[currentPos]);

    }


    //check the open list to see if the path points are within the list
    bool CheckOpenList(STR_ID currentNodeID, STR_ID NeighbourID)
    {
        bool exists = false;
        for(int i=0;i< open.Count;i++)
        {
            if(open[i].ReturningParentNode()!=null)
            {
                STR_ID currentPointID = open[i].ReturningMainNode().returnID();
                STR_ID neighbourPointID = open[i].ReturningParentNode().returnID();

                if(currentNodeID.Compare(currentPointID) && NeighbourID.Compare(neighbourPointID))
                {
                    exists = true;
                }

                if (currentNodeID.Compare(neighbourPointID) && NeighbourID.Compare(currentPointID))
                {
                    exists = true;
                }

            }
        }
        return exists;
    }


    //check the closed list to see if the path points are within the list
    bool CheckClosedList(STR_ID currentNodeID, STR_ID NeighbourID)
    {
        bool exists = false;
        for (int i = 0; i < closed.Count; i++)
        {
            if (closed[i].ReturningParentNode() != null)
            {
                STR_ID currentPointID = closed[i].ReturningMainNode().returnID();
                STR_ID neighbourPointID = closed[i].ReturningParentNode().returnID();

                if (currentNodeID.Compare(currentPointID) && NeighbourID.Compare(neighbourPointID))
                {
                    exists = true;
                }

                if (currentNodeID.Compare(neighbourPointID) && NeighbourID.Compare(currentPointID))
                {
                    exists = true;
                }

            }
        }

        return exists;
    }


    //sort through the closed list to find the final path
    void FindPath()
    {
        finalPathQueue.Enqueue(closed[pathEnd].ReturningMainNode());
        SCR_NodeClass CurrentParent = closed[pathEnd].ReturningParentNode();
        
        while (CurrentParent != null)
        {
            var currentPosition = ParentSearch(CurrentParent);

            if (closed[currentPosition].ReturningParentNode() == null)
            {
                break;
            }
            finalPathQueue.Enqueue(closed[currentPosition].ReturningMainNode());
            CurrentParent = closed[currentPosition].ReturningParentNode();
        }

        int PathSize = finalPathQueue.Count;
        for (int i = 0; i < PathSize; i++)
        {
            finalMapPathway.Add(finalPathQueue.Dequeue());

        }
        finalPathQueue.Clear();
    }

    //search through the closed list to find a nodes parent node
    int ParentSearch(SCR_NodeClass Parent)
    {
        int pos = 0;
        for (int i = 0; i < closed.Count; i++)
        {
            if (closed[i].ReturningMainNode() == Parent)
            {
                pos = i;
                break;
            }
        }

        return pos;
    }


}

//Class used to store a nodes main class, as well as its parent
class CalculatePath
{
    SCR_NodeClass mainNode, previousNode;

    public CalculatePath(SCR_NodeClass Position, SCR_NodeClass Parent)
    {
        mainNode = Position;
        previousNode = Parent;

    }

    public SCR_NodeClass ReturningMainNode()
    {
        return mainNode;
    }

    public SCR_NodeClass ReturningParentNode()
    {
        return previousNode;
    }


}

