using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GunTypes
{
    Primary,
    Secondary,
    Infinite,
    Throwable
};





public enum ElementalEffect
{
    Fire,
    Oil,
    Normal,
    Destructor
}


[System.Serializable]
public class SCR_GunTypes 
{

    [Header("Clip Range")]




    [Range(0, 100)]
    [SerializeField]
    int MinClipSize;
    [Range(0, 100)]
    [SerializeField]
    int MaxClipSize;


    [Header("DPS Range")]

    [Range(0, 100)]
    [SerializeField]
    float MinDPS;


    [Range(0, 100)]
    [SerializeField]
    float MaxDPS;


    [Header("Accuracy Range")]

    [Range(0, 100)]
    [SerializeField]
    float MinAccuracy;
    [Range(0, 100)]
    [SerializeField]
    float MaxAccuracy;


    [Header("Fire rate Range")]

    [Range(0, 100)]
    [SerializeField]
    float MinFireRate;
    [Range(0, 100)]
    [SerializeField]
    float MaxFireRate;


    [Header("Gun Information")]
    [Space(10)]
    [SerializeField]
    GameObject BodyPrefab;
    [Space(10)]
    [SerializeField]
    string TypeName;
    
    [SerializeField]
    GunTypes WeaponSlot;


    [SerializeField]
    WeaponType weaponType;




    [Header("Weapon Parts")]

    [Header("Scope Types")]
    [SerializeField]
    public List<SCR_WeaponPartsClass> scopes;

    [Header("Under Barrel Types")]
    [SerializeField]
    public List<SCR_WeaponComponent> underBarrel;

    [Header("Barrel Types")]
    [SerializeField]
    public List<SCR_WeaponComponent> barells;

    [Header("Clip Types")]
    [SerializeField]
    public List<SCR_WeaponPartsRangedClass> clips;

   


    public SCR_GunTypes()
    {

        MinClipSize = 0;
        MaxClipSize = 0;

        MinDPS = 0;
        MaxDPS = 0;

        MinAccuracy = 0;
        MaxAccuracy = 0;

        MinFireRate = 0;
        MaxFireRate = 0;

        TypeName = "";
    }


    public GameObject ReturnBody()
    {
        return BodyPrefab;
    }

    public GunComponentValues ReturnBaseClip()
    {
        GunComponentValues ClipValues = new GunComponentValues();
        ClipValues.MIN = MinClipSize;
        ClipValues.MAX = MaxClipSize;
        return ClipValues;
    }

    public GunComponentValues ReturnBaseDPS()
    {
        GunComponentValues DPSValues = new GunComponentValues();
        DPSValues.MIN = MinDPS;
        DPSValues.MAX = MaxDPS;
        return DPSValues;
    }

    public GunComponentValues ReturnBaseFireRate()
    {
        GunComponentValues FireRateValues = new GunComponentValues();
        FireRateValues.MIN = MinFireRate;
        FireRateValues.MAX = MaxFireRate;
        return FireRateValues;
    }

    public GunComponentValues ReturnBaseAccuracy()
    {
        GunComponentValues AccuracyValues = new GunComponentValues();
        AccuracyValues.MIN = MinAccuracy;
        AccuracyValues.MAX = MaxAccuracy;
        return AccuracyValues;
    }


    public string ReturnName()
    {
        return TypeName;
    }


    public GunTypes ReturnGunTypes()
    {
        return WeaponSlot;
    }

    public WeaponType ReturnWeaponsType()
    {
        return weaponType;
    }

}


public struct GunComponentValues
{
    public float MIN;
    public float MAX;
}
 
