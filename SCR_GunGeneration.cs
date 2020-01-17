using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SCR_GunGeneration : MonoBehaviour
{
   
    [Header("Types of Weapons")]
    [SerializeField]
    List<SCR_GunTypes> typesOfGuns;


    [Header("Weapon Rarity")]
    [SerializeField]
    List<WeaponRatings> weaponRates;

    [SerializeField]
    GameObject Startpos;


    GameObject SpawnedGun;

    [Header("Final Gun")]
    [SerializeField]
    SCR_GunClass Gun;


    [SerializeField] private GameObject player;

    [SerializeField] private GameObject pickupObj;

    private Vector3 spawnPos;

    [Space(10)]
    [Header("List of Legendary Abilites")]
    [SerializeField]
    private List<SCR_Ability> LegendaryAbilities;

    [Space(20)] [Header("Sound Effects")]
    [SerializeField] private AudioClip weaponPickupEffect;

    private AudioSource mySource;

    void Start()
    {
        mySource = GetComponent<AudioSource>();
    }

    //Pass in the gun values type, and it will return the class than can be used to construct it
    //If you use functions such as   GunComponentValues GunFireRateValues = typesOfGuns[SelectedGun].ReturnFireRate() you can access the min/max of values
    public SCR_GunTypes RetrieveGunType(int type)
    {
        return typesOfGuns[type];
    }

    //pass in the guns current rarity and it will return the value used to affect weapons
    public float RetrieveRarityStatAffect(Rarity rarity)
    {
        int raritySelection = 0;
        for (int i = 0; i < weaponRates.Count; i++)
        {
            if (weaponRates[i].rarityType == rarity)
            {
                raritySelection = i;
                break;
            }
        }

        return weaponRates[raritySelection].statsAffect;
    }

    public void SpawnGun(GameObject mPlayerInteracting, Transform mParentObject)
    {
        GunValues currentGun = Gun.ReturnGunValues();
        SpawnedGun = Instantiate(typesOfGuns[currentGun.TYPE].ReturnBody(), spawnPos, Quaternion.identity);

        IWeapon currentWeaponInterface = (IWeapon)SpawnedGun.GetComponent(typeof(IWeapon));
        GunPositions currentGunPos = currentWeaponInterface.returnPositions();
        GameObject ScopeObj = currentGunPos.scopePosition;
        GameObject underBarrelObj = currentGunPos.underBarrelPosition;
        GameObject ClipObj = currentGunPos.clipPosition;
        GameObject barrelObj = currentGunPos.barrelPosition;

        GameObject currentGunComponent;
        List<SCR_WeaponPartsClass> scopes = typesOfGuns[currentGun.TYPE].scopes;
        if (scopes[currentGun.SCOPE].ReturnPart())
        {
            Destroy(ScopeObj.transform.GetChild(0).gameObject);
            currentGunComponent = Instantiate(scopes[currentGun.SCOPE].ReturnPart(), ScopeObj.transform.position, Quaternion.identity);
            if (currentGun.GUNTYPE == WeaponType.pistol)
            {
                if (currentGun.SCOPE >= 2)
                {
                    currentGunComponent.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                else
                {
                    currentGunComponent.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                }
            }
               
            currentGunComponent.transform.parent = ScopeObj.transform;
            currentGunComponent.transform.position = ScopeObj.transform.position;
            Vector3 pos = currentGunComponent.transform.position;
            Vector3 scopeSize = currentGunComponent.GetComponent<Renderer>().bounds.size;
            pos.y += (scopeSize.y / 2);
            pos.x += (scopeSize.z / 2);
            currentGunComponent.transform.position = pos;
        }

        List<SCR_WeaponComponent> underBarrels = typesOfGuns[currentGun.TYPE].underBarrel;

        if (underBarrels.Count > 0)
        {
            if (underBarrels[currentGun.UNDERBARREL].ReturnPart())
            {
                currentGunComponent = Instantiate(underBarrels[currentGun.UNDERBARREL].ReturnPart(), underBarrelObj.transform.position, Quaternion.identity);
                currentGunComponent.transform.parent = underBarrelObj.transform;
                currentGunComponent.transform.position = underBarrelObj.transform.position;
                Vector3 pos = currentGunComponent.transform.position;
                Vector3 size = currentGunComponent.transform.GetComponent<Renderer>().bounds.size;
                pos.y -= (size.y / 2);
                currentGunComponent.transform.position = pos;
            }
        }       

        for (int i = 0; i < ClipObj.transform.childCount; i++)
        {
            if (i <= currentGun.CLIP)
            {
                ClipObj.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                ClipObj.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        List<SCR_WeaponComponent> barrel = typesOfGuns[currentGun.TYPE].barells;
        if (barrel.Count > 0)
        {
            if (barrel[currentGun.BARREL].ReturnPart())
            {
                currentGunComponent = Instantiate(barrel[currentGun.BARREL].ReturnPart(), barrelObj.transform.position, Quaternion.identity);
                currentGunComponent.transform.parent = barrelObj.transform;
                currentGunComponent.transform.position = barrelObj.transform.position;
                Vector3 pos = currentGunComponent.transform.position;
                Vector3 size = currentGunComponent.transform.GetComponent<Renderer>().bounds.size;
                pos.x += (size.x / 2);
                Transform barrelEnd = currentWeaponInterface.ReturnGunEnd();
                Vector3 currentPosition = barrelEnd.transform.position;
                currentPosition.x += size.x;
                barrelEnd.transform.position = currentPosition;
                currentGunComponent.transform.position = pos;
            }
        }
     
        SCR_Ability ability = null;
        if (currentGun.ABILITYTYPE != LegendaryAbilities.Count)
        {
            ability = LegendaryAbilities[currentGun.ABILITYTYPE];
            ability.SetPlayerParent(mPlayerInteracting);
        }

        currentWeaponInterface.SetGunValues(Gun,mPlayerInteracting, ability);
        SpawnedGun.transform.parent = mParentObject;
        SpawnedGun.transform.rotation = new Quaternion(0,0,0,0);       
        SpawnedGun.transform.position = mParentObject.transform.position;
        mPlayerInteracting.GetComponent<SCR_CharacterInventory>().AddWeapon(Gun,SpawnedGun);

        SCR_UIManager.instance.bWeaponGenerated = false;
        SCR_UIManager.instance.generatedWeaponClass = null;
        SCR_UIManager.instance.WeaponPickupUI("Player1", false);
        SCR_UIManager.instance.WeaponPickupUI("Player2", false);
        mySource.PlayOneShot(weaponPickupEffect);
    }

    //use this to spawn the last created gun at a specific vector3 position
    public GameObject SpawnCreatedGun(Vector3 mPosition)
    {
        GunValues currentGun = Gun.ReturnGunValues();
        SpawnedGun = Instantiate(typesOfGuns[currentGun.TYPE].ReturnBody(), spawnPos, Quaternion.identity);
        IWeapon currentWeaponInterface = (IWeapon)SpawnedGun.GetComponent(typeof(IWeapon));
        GunPositions currentGunPos = currentWeaponInterface.returnPositions();
        GameObject ScopeObj = currentGunPos.scopePosition;
        GameObject underBarrelObj = currentGunPos.underBarrelPosition;
        GameObject ClipObj = currentGunPos.clipPosition;
        GameObject barrelObj = currentGunPos.barrelPosition;

        GameObject currentGunComponent;
        List<SCR_WeaponPartsClass> scopes = typesOfGuns[currentGun.TYPE].scopes;

        if (scopes[currentGun.SCOPE].ReturnPart())
        {
                Destroy(ScopeObj.transform.GetChild(0).gameObject);
                currentGunComponent = Instantiate(scopes[currentGun.SCOPE].ReturnPart(), ScopeObj.transform.position, Quaternion.identity);
            if (currentGun.GUNTYPE == WeaponType.pistol)
            {
                if (currentGun.SCOPE >= 2)
                {
                    currentGunComponent.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                }
                else
                {
                    currentGunComponent.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                }
            }
            currentGunComponent.transform.parent = ScopeObj.transform;
                currentGunComponent.transform.position = ScopeObj.transform.position;
                Vector3 pos = currentGunComponent.transform.position;
                Vector3 scopeSize = currentGunComponent.GetComponent<Renderer>().bounds.size;
                pos.y += (scopeSize.y / 2);
                pos.x += (scopeSize.z / 2);
                currentGunComponent.transform.position = pos;


        }


        List<SCR_WeaponComponent> underBarrels = typesOfGuns[currentGun.TYPE].underBarrel;

        if (underBarrels.Count > 0)
        {
            if (underBarrels[currentGun.UNDERBARREL].ReturnPart())
            {
                currentGunComponent = Instantiate(underBarrels[currentGun.UNDERBARREL].ReturnPart(), underBarrelObj.transform.position, Quaternion.identity);
                currentGunComponent.transform.parent = underBarrelObj.transform;
                currentGunComponent.transform.position = underBarrelObj.transform.position;
                Vector3 pos = currentGunComponent.transform.position;
                Vector3 size = currentGunComponent.transform.GetComponent<Renderer>().bounds.size;
                pos.y -= (size.y / 2);
                currentGunComponent.transform.position = pos;
            }
        }





        for (int i = 0; i < ClipObj.transform.childCount; i++)
        {
            if (i <= currentGun.CLIP)
            {
                ClipObj.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                ClipObj.transform.GetChild(i).gameObject.SetActive(false);
            }
        }


        List<SCR_WeaponComponent> barrel = typesOfGuns[currentGun.TYPE].barells;
        if (barrel.Count > 0)
        {
            if (barrel[currentGun.BARREL].ReturnPart())
            {
                currentGunComponent = Instantiate(barrel[currentGun.BARREL].ReturnPart(), barrelObj.transform.position, Quaternion.identity);
                currentGunComponent.transform.parent = barrelObj.transform;
                currentGunComponent.transform.position = barrelObj.transform.position;
                Vector3 pos = currentGunComponent.transform.position;
                Vector3 size = currentGunComponent.transform.GetComponent<Renderer>().bounds.size;
                pos.x += (size.z / 2);
                currentGunComponent.transform.position = pos;
                Transform barrelEnd = currentWeaponInterface.ReturnGunEnd();
                Vector3 currentPosition = barrelEnd.transform.position;
                currentPosition.x += size.x;
                barrelEnd.transform.position = currentPosition;
                currentGunComponent.transform.position = pos;

            }
        }
        SpawnedGun.transform.position = mPosition;

        return SpawnedGun;
    }

    public SCR_Ability ReturnAbilityType(int mAbilityType)
    {
        SCR_Ability possibleAbility = null;
        if (mAbilityType < LegendaryAbilities.Count)
        {
            possibleAbility = LegendaryAbilities[mAbilityType];
        }
        return possibleAbility;
    }

    public SCR_GunClass CreateGunFromDifferenceEngine(bool randomGeneration, Rarity rarity)
    {
        Generate(randomGeneration, rarity);   
        return Gun;
    }

    private void Generate(bool random, Rarity rarity)
    {
        int typeRNG = Random.Range(0, typesOfGuns.Count);
        int SelectedGun = typeRNG;

      

        float ROF, DPS, CLIP, ACC;

        GunComponentValues GunFireRateValues = typesOfGuns[SelectedGun].ReturnBaseFireRate();
        ROF = Random.Range(GunFireRateValues.MIN, GunFireRateValues.MAX);

        bool HighFireRate = false;

        float MidwayValues = GunFireRateValues.MIN + ((GunFireRateValues.MAX - GunFireRateValues.MIN) / 2);
        if (ROF >= MidwayValues)
        {
            HighFireRate = true;
        }


        GunComponentValues GunAccuracyValue = typesOfGuns[SelectedGun].ReturnBaseAccuracy();
        GunComponentValues GunClipValue = typesOfGuns[SelectedGun].ReturnBaseClip();
        GunComponentValues GunDPSValue = typesOfGuns[SelectedGun].ReturnBaseDPS();

        if (HighFireRate)
        {
            MidwayValues = ((GunAccuracyValue.MAX - GunAccuracyValue.MIN) / 2);
            GunAccuracyValue.MAX -= MidwayValues;

            MidwayValues = ((GunDPSValue.MAX - GunDPSValue.MIN) / 2);
            GunDPSValue.MAX -= MidwayValues;

            MidwayValues = ((GunClipValue.MAX - GunClipValue.MIN) / 2);
            GunClipValue.MIN += MidwayValues;
        }
        else
        {
            MidwayValues = ((GunAccuracyValue.MAX - GunAccuracyValue.MIN) / 2);
            GunAccuracyValue.MIN += MidwayValues;

            MidwayValues = ((GunDPSValue.MAX - GunDPSValue.MIN) / 2);
            GunDPSValue.MIN += MidwayValues;

            MidwayValues = ((GunClipValue.MAX - GunClipValue.MIN) / 2);
            GunClipValue.MAX -= MidwayValues;
        }



        List<SCR_WeaponPartsClass> scopes = typesOfGuns[SelectedGun].scopes;
        ///Decide if a scope will be added, if so then affect the guns accuracy rate to suit
        int ScopeSelected = Random.Range(0, scopes.Count);      
        float AccuracyAddOn = (((GunAccuracyValue.MAX - GunAccuracyValue.MIN) / 2) / 100) * scopes[ScopeSelected].ReturnPercentageIncrease();
        GunAccuracyValue.MIN += AccuracyAddOn;



        int underBarrelSelected = 0;
        List<SCR_WeaponComponent> underBarrels = typesOfGuns[SelectedGun].underBarrel;
        ///Decide if a scope will be added, if so then affect the guns accuracy rate to suit///
        if(underBarrels.Count>0)
        {
            underBarrelSelected = Random.Range(0, underBarrels.Count);

            for (int i = 0; i < underBarrels[underBarrelSelected].statsComponentAffects.Count; i++)
            {
                float statAffectAddOn = 0;
                STR_statsAffect currentComponent = underBarrels[underBarrelSelected].statsComponentAffects[i];
                statAffect currentStatAffect = currentComponent.componentStatType;
                switch (currentStatAffect)
                {
                    case statAffect.accuracy:
                       statAffectAddOn = (((GunAccuracyValue.MAX - GunAccuracyValue.MIN) / 2) / 100) * currentComponent.percentage;
                       GunAccuracyValue.MIN += statAffectAddOn;
                       break;


                    case statAffect.damage:
                       statAffectAddOn = (((GunDPSValue.MAX - GunDPSValue.MIN) / 2) / 100) * currentComponent.percentage;
                        GunDPSValue.MIN += statAffectAddOn;
                       break;


                    case statAffect.clipSize:
                        statAffectAddOn = (((GunClipValue.MAX - GunClipValue.MIN) / 2) / 100) * currentComponent.percentage;
                        GunClipValue.MIN += statAffectAddOn;
                        break;


                    case statAffect.rateOfFire:
                        statAffectAddOn = (((GunFireRateValues.MAX - GunFireRateValues.MIN) / 2) / 100) * currentComponent.percentage;
                        GunFireRateValues.MIN += statAffectAddOn;
                        break;
                }
            }
        }




        int barrelSelected = 0;
        List<SCR_WeaponComponent> barrel = typesOfGuns[SelectedGun].barells;
        ///Decide if a new clip will be added, if so then affect the guns clip size to suit      
        if (barrel.Count > 0)
        {
            barrelSelected = Random.Range(0, barrel.Count);
            for (int i = 0; i < underBarrels[underBarrelSelected].statsComponentAffects.Count; i++)
            {
                float statAffectAddOn = 0;
                STR_statsAffect currentComponent = underBarrels[underBarrelSelected].statsComponentAffects[i];
                statAffect currentStatAffect = currentComponent.componentStatType;
                switch (currentStatAffect)
                {
                    case statAffect.accuracy:
                        statAffectAddOn = (((GunAccuracyValue.MAX - GunAccuracyValue.MIN) / 2) / 100) * currentComponent.percentage;
                        GunAccuracyValue.MIN += statAffectAddOn;
                        break;


                    case statAffect.damage:
                        statAffectAddOn = (((GunDPSValue.MAX - GunDPSValue.MIN) / 2) / 100) * currentComponent.percentage;
                        GunDPSValue.MIN += statAffectAddOn;
                        break;


                    case statAffect.clipSize:
                        statAffectAddOn = (((GunClipValue.MAX - GunClipValue.MIN) / 2) / 100) * currentComponent.percentage;
                        GunClipValue.MIN += statAffectAddOn;
                        break;


                    case statAffect.rateOfFire:
                        statAffectAddOn = (((GunFireRateValues.MAX - GunFireRateValues.MIN) / 2) / 100) * currentComponent.percentage;
                        GunFireRateValues.MIN += statAffectAddOn;
                        break;
                }
            }
        }   











        ACC = Random.Range(GunAccuracyValue.MIN, GunAccuracyValue.MAX);
        DPS = Random.Range(GunDPSValue.MIN, GunDPSValue.MAX);
        CLIP = Random.Range(GunClipValue.MIN, GunClipValue.MAX);






        List<SCR_WeaponPartsRangedClass> clips = typesOfGuns[SelectedGun].clips;
        ///Decide if a new clip will be added, if so then affect the guns clip size to suit
        int clipSelected = 0;
        GunComponentValues maxValues = clips[clips.Count-1].ReturnRangedValue();
        if (CLIP > maxValues.MAX)
        {
            clipSelected = clips.Count - 1;
        }
        else
        {
            if (clips.Count > 0)
            {
                for (int i = 0; i < clips.Count; i++)
                {
                    GunComponentValues ClipValues = clips[i].ReturnRangedValue();

                    if (CLIP >= ClipValues.MIN && CLIP < ClipValues.MAX && (i + 1) == clips.Count)
                    {
                        clipSelected = i;
                    }
                }

            }
        }


       

        int WeaponsRating = 0;
        if (random)
        {
            int RatingRNG = Random.Range(0, 101);
           
            for (int i = weaponRates.Count - 1; i >= 0; i--)
            {
                if (RatingRNG > weaponRates[i].Rating && WeaponsRating < i)
                {
                    WeaponsRating = i;
                }
            }
        }
        else
        {
            for (int i = 0; i < weaponRates.Count; i++)
            {
                if (weaponRates[i].rarityType ==rarity)
                {
                    WeaponsRating = i;
                    break;
                }
            }
        }



        ACC *= weaponRates[WeaponsRating].statsAffect;
        DPS *= weaponRates[WeaponsRating].statsAffect;
        CLIP *= weaponRates[WeaponsRating].statsAffect;
        ROF *= weaponRates[WeaponsRating].statsAffect;


        GunTypes currentGunType = typesOfGuns[SelectedGun].ReturnGunTypes();
        WeaponType currentWeaponType = typesOfGuns[SelectedGun].ReturnWeaponsType();


       ElementalEffect currentWeaponsEffect = SelectEffect();


        int weaponAbility = LegendaryAbilities.Count;
        bool hasAbility = false;
        if (weaponRates[WeaponsRating].rarityType ==  Rarity.Legendary)
        {
            weaponAbility = Random.Range(0, LegendaryAbilities.Count);
            hasAbility = true;
        }




        Gun = new SCR_GunClass();
        Gun.SetGunModel(SelectedGun, ScopeSelected, clipSelected, underBarrelSelected, barrelSelected, currentGunType, currentWeaponType , currentWeaponsEffect, weaponAbility, hasAbility);
        Gun.SetGunStats((int)CLIP, DPS, ROF, ACC);
        Gun.SetRarity(weaponRates[WeaponsRating].rarityType);

       



    }

    float GenerateRates(float startPositon, float scalar)
    {
        float Value = Random.Range(startPositon, startPositon + scalar);

        return Value;
    }

    ElementalEffect SelectEffect()
    {     
        int rng = Random.Range(0, 4);
        ElementalEffect effect = ElementalEffect.Normal;
        switch (rng)
        {
            case 0:  effect = ElementalEffect.Normal; break;
            case 1:  effect = ElementalEffect.Fire; break;
            case 2:  effect = ElementalEffect.Oil; break;
            case 3:  effect = ElementalEffect.Destructor; break;
        }

        return effect;
    } 

}


[System.Serializable]
struct WeaponRatings
{
    [Space(10)]
    public string type;

    [Space(10)]
    [Range(0, 100)]
    public int Rating;


    public Rarity rarityType;
    [Space(10)]
    [Range(1, 2)]
    public float statsAffect;
    [Space(10)]
    public Color ColorType;
}

public struct GunValues
{
    public float ACCURACY;
    public float DPS;
    public int CLIPSIZE;
    public float RATEOFFIRE;
    public Rarity RARITY;
    public int SCOPE;
    public int CLIP;
    public int UNDERBARREL;
    public int BARREL;
    public int TYPE;
    public WeaponType GUNTYPE;
    public GunTypes SLOTTYPE;
    public ElementalEffect EFFECT;
    public bool ABILITY;
    public int ABILITYTYPE;
}

public struct GunPositions
{
    public GameObject scopePosition, clipPosition, underBarrelPosition, barrelPosition;
}