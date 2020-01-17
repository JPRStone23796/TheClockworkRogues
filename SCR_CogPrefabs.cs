using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
[CreateAssetMenu(fileName = "CogPrefabs", menuName = "Prefabs/Cogs")]
public class SCR_CogPrefabs : ScriptableObject
{
    public List<GameObject> CogPrefabsList;

    public GameObject cogObj;

}
