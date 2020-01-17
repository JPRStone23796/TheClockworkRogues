using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SCR_WeaponPartsClass
{
    [SerializeField]
    GameObject Part;

    [Space(10)]
    [Range(-100,100)]
    [SerializeField]
    float PercentageIncrease;

    public SCR_WeaponPartsClass()
    {
        Part = null;
        PercentageIncrease = 0.0f;
    }

    public GameObject ReturnPart()
    {
        return Part;
    }



    public float ReturnPercentageIncrease()
    {
        return PercentageIncrease;
    }


 
}
