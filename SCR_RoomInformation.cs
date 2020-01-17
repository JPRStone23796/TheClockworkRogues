using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Room Information", menuName = "Rooms/Information")]
public class SCR_RoomInformation : ScriptableObject
{
    [Header("Types of Grunts")]
    public STR_GruntTypes[] gruntSpeedTypes;

    [Header("Amount of each type of grunt")]
    public STR_GruntWaves waves;


    [Header("Each enemy prefab")]
    public List<GameObject> typesOfEnemies;


}
