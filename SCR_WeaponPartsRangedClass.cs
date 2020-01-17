using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SCR_WeaponPartsRangedClass
{


    [Space(10)]
    [Range(0, 100)]
    [SerializeField]
    float MinimumPartValue, MaxmimumPartValue;

    public SCR_WeaponPartsRangedClass()
    {
        MinimumPartValue = 0.0f;
        MaxmimumPartValue = 0.0f;
    }

    public GunComponentValues ReturnRangedValue()
    {
        GunComponentValues values = new GunComponentValues();
        values.MIN = MinimumPartValue;
        values.MAX = MaxmimumPartValue;
        return values;
    }



}
