using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum statAffect
{
    accuracy,
    damage,
    rateOfFire,
    clipSize

}


[System.Serializable]
public class SCR_WeaponComponent
{
    public List<STR_statsAffect> statsComponentAffects;

    [SerializeField]
    private GameObject body;
   
    public GameObject ReturnPart()
    {
        return body;
    }


}


[System.Serializable]
public struct STR_statsAffect
{
    [SerializeField]
    public statAffect componentStatType;
 
    [Header("Percentage affect")]
    [SerializeField]
    public float percentage;
}