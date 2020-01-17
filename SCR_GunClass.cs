using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    Common, Uncommon, Rare, Legendary
}

[System.Serializable]
public class SCR_GunClass
{
    [SerializeField] int body, scope, clip, underBarrel, barrel;
    [SerializeField] int ClipSize;
    [SerializeField] float DamagePerShot, Accuracy;
    [SerializeField] float RateOfFire;
    [SerializeField] Rarity Rarity;
    [SerializeField] private int typeOfAbility;


    [SerializeField] private WeaponType typeOfWeapon;
    [SerializeField] private GunTypes WeaponSlot;

    [SerializeField] private ElementalEffect effect;

    private bool Ability = false;


   




    public SCR_GunClass()
    {
        body = 0;
        scope = 0;
        clip = 0;
        underBarrel = 0;
        barrel = 0;
        ClipSize = 0;
        DamagePerShot = 0;
        RateOfFire = 0.0f;
        Accuracy = 0;
        Rarity = 0;
        typeOfWeapon = WeaponType.pistol;
    }

    public void SetGunModel(int bodyType, int scopeType, int clipType, int underBarrelType, int stockType,
GunTypes type, WeaponType weaponType, ElementalEffect typeOfEffect, int mtypeOfAbility, bool mHasAbility)
    {
        body = bodyType;
        scope = scopeType;
        clip = clipType;
        underBarrel = underBarrelType;
        barrel = stockType;
        WeaponSlot = type;
        typeOfWeapon = weaponType;
        effect = typeOfEffect;
        Ability = mHasAbility;
        typeOfAbility = mtypeOfAbility;
    }

    public void SetGunStats(int GunClip, float DPS, float FireRate, float GunAccuracy)
    {
        ClipSize = GunClip;
        DamagePerShot = DPS;
        RateOfFire = FireRate;
        Accuracy = GunAccuracy;
    }

    public void SetRarity(Rarity rarityType)
    {
        Rarity = rarityType;
    }


 



    public GunTypes returnType()
    {
        return WeaponSlot;
    }


    public GunValues ReturnGunValues()
    {
        GunValues currentValues = new GunValues();
        currentValues.ACCURACY = Accuracy;
        currentValues.DPS = DamagePerShot;
        currentValues.CLIPSIZE = ClipSize;
        currentValues.RATEOFFIRE = RateOfFire;
        currentValues.RARITY = Rarity;
        currentValues.TYPE = body;
        currentValues.SCOPE = scope;
        currentValues.BARREL = barrel;
        currentValues.UNDERBARREL = underBarrel;
        currentValues.CLIP = clip;
        currentValues.SLOTTYPE = WeaponSlot;
        currentValues.GUNTYPE = typeOfWeapon;
        currentValues.EFFECT = effect;
        currentValues.ABILITY = Ability;
        currentValues.ABILITYTYPE = typeOfAbility;
        return currentValues;
    }

}
