using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using XboxCtrlrInput;

public class SCR_CharacterInventory : MonoBehaviour
{
    [Space(10)]
    [Tooltip("Represents the Amount of slots there should be. Default is 4")]
    [SerializeField] int numberOfWeapons = 4;

    [Space(10)]
    [Tooltip("Represents each weapon slot")]
    [SerializeField] private  STR_GunInventory[] _gunInventory;

    [Space(10)]
    [Tooltip("Represents the length of time the crit timer will be applied")]
    [SerializeField] private float critTimer = 5.0f;

    [Space(10)]
    [Tooltip("Represents the damage multiplier that will be applied while the crit bonus is appliable")]
    [SerializeField] private float critDamageMultiplier;

    [Space(10)]
    [Tooltip("Represents the current grenade prefab each player has, will need to be updated to switch between all 3")]
    [SerializeField] private GameObject grenade;

    [Space(10)]
    [Tooltip("Represents the infinite pistol, the player starting pistol")]
    [SerializeField] private GameObject infinitePistolPrefab;

    [Space(10)] [Header("Audio")]
    [SerializeField] private AudioClip[] weaponSwitchingClips;

    [SerializeField] private AudioClip failedWeaponSwitchClip;

    [SerializeField] private AudioClip grenadeSwitchingClip;

    [SerializeField] private AudioSource weaponSwitchingSource;

    public SCR_PlayerClass.Player playerStats;


    private STR_CurrentPlayerBuffs currentPlayerBuffs;

    private int _currentlySelectedWeapon;
    private IWeapon _currentWeaponInterface;
    private SCR_PlayerAttack _attackScript;
    private SCR_PlayerInput _inputScript;
    private bool _isReloading = false, _CritAttempted, _grenadeSelected = false;
    private float MAX_TRG_SCL = 1.21f;
    private int grenadePreviousWeapon = 0;
    private bool bLeftTriggerDown = false;

    [HideInInspector] public bool bPlayerHasGun = false;
    bool bCritActive = false;

    private SCR_UIManager uiManagerScript;
    private SCR_PlayerWorldSpaceUI playerWorldSpaceUIScr;
    SCR_PlayerWorldSpaceUI.WeaponSwitchDirection switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Up;

    private float healthMultiplier = 1.0f, reloadMultiplier = 1.0f, costMultiplier = 1.0f, speedMultipler = 1.0f;
    private bool bHasSwitchedWeapons = false;
    public bool bTutorialCanSwitchWeapons = false;
    private bool bHasReloaded = false;
    public bool bTutorialCanReload = false;

    public float CurrentPlayersCostMultiplier
    {
        get { return costMultiplier;}
    }

    private IEnumerator normalGauge,critGauge, critUpgrade , ammoGauge;

    void Awake()
    {
        _attackScript = gameObject.GetComponent<SCR_PlayerAttack>();
        _inputScript = GetComponent<SCR_PlayerInput>();
        uiManagerScript = GameObject.FindGameObjectWithTag("UIManager").GetComponent<SCR_UIManager>();

        //reset playerWeaponUI once for both players
        if (gameObject.tag == "Player1")
        {
            SCR_UIManager.instance.ResetWeaponSlots();
        }

        _gunInventory = new STR_GunInventory[numberOfWeapons];
        for (int i = 0; i < numberOfWeapons; i++)
        {
            _gunInventory[i].currentAmmo = 100;
        }

        _gunInventory[numberOfWeapons - 1].gunObject = grenade;
        _gunInventory[numberOfWeapons - 1].currentAmmo = 5;


        Transform gunPos = _inputScript.gunPosition;
        GameObject infiniteGun = Instantiate(infinitePistolPrefab, gunPos.transform.position, new Quaternion(0,180,0,0));
        infiniteGun.transform.parent = gunPos;
        infiniteGun.transform.rotation = new Quaternion(0,0,0,0);
        _gunInventory[2].gunObject = infiniteGun;
        _gunInventory[2].currentAmmo = 100;
        uiManagerScript.SetCurrentAmmoText(gameObject.tag, 0.0f, 0, 0, true);
        _currentWeaponInterface = (IWeapon)infiniteGun.GetComponent(typeof(IWeapon));
        _currentWeaponInterface.SetParent(gameObject);
        _attackScript.UpdateWeaponInterface(_currentWeaponInterface);
        _currentlySelectedWeapon = 2;
        _inputScript.SpeedMultiplier = speedMultipler;
        GetComponent<SCR_PlayerHealth>().HealthMultiplier = healthMultiplier;
        weaponSwitchingSource = GetComponent<AudioSource>();
        uiManagerScript.SwitchGunImages(gameObject.tag, false, ThrowableType.FireGrenade, true);
        uiManagerScript.SwitchSelectedWeaponUI(gameObject.tag, _currentlySelectedWeapon);
        playerWorldSpaceUIScr = GetComponent<SCR_PlayerWorldSpaceUI>();

        MAX_TRG_SCL = GetComponent<SCR_PlayerAttack>().MAX_TRG_SCL;
    }

    public Dictionary<WeaponType, int> GetInventoryTypes()
    { // Return a dictionary of the weapon types and their max clip size
        Dictionary<WeaponType, int> returnList = new Dictionary<WeaponType, int>();

        foreach (STR_GunInventory gun in _gunInventory)
        {
            if (gun.gunObject != null)
            {
                IWeapon gunInterface = gun.gunObject.GetComponent<IWeapon>();
                
                if (gunInterface != null)
                {
                    WeaponType type = gunInterface.RetrieveGunValues().ReturnGunValues().GUNTYPE;

                    if (!returnList.ContainsKey(type))
                    {
                        returnList.Add(type, gunInterface.RetrieveGunValues().ReturnGunValues().CLIPSIZE);
                    }
                }
            }
        }

        return returnList;
    }

    public void SetPlayerBuff(STR_CurrentPlayerBuffs mCurrentBuffs)
    {
        currentPlayerBuffs = mCurrentBuffs;
        healthMultiplier = 1.0f;
        reloadMultiplier = 1.0f;
        costMultiplier = 1.0f;
        speedMultipler = 1.0f;

        switch (mCurrentBuffs.PositiveBuff.typeOfBuff)
        {
              case BuffType.Health: healthMultiplier = mCurrentBuffs.PositiveBuff.abilityAffectValue; break;
              case BuffType.Cost: costMultiplier = mCurrentBuffs.PositiveBuff.abilityAffectValue; break;
              case BuffType.Reload: reloadMultiplier = mCurrentBuffs.PositiveBuff.abilityAffectValue; break;
              case BuffType.Speed: speedMultipler = mCurrentBuffs.PositiveBuff.abilityAffectValue; break;
        }


        switch (mCurrentBuffs.NegativeBuff.typeOfBuff)
        {
            case BuffType.Health: healthMultiplier = mCurrentBuffs.NegativeBuff.abilityAffectValue; break;
            case BuffType.Cost: costMultiplier = mCurrentBuffs.NegativeBuff.abilityAffectValue; break;
            case BuffType.Reload: reloadMultiplier = mCurrentBuffs.NegativeBuff.abilityAffectValue; break;
            case BuffType.Speed: speedMultipler = mCurrentBuffs.NegativeBuff.abilityAffectValue; break;
        }

        GetComponent<SCR_PlayerInput>().SpeedMultiplier = speedMultipler;
        GetComponent<SCR_PlayerHealth>().HealthMultiplier = healthMultiplier;
    }

    public void Update()
    {
        WeaponSwitching();
        Reload();   
    }

    public void AddWeapon(SCR_GunClass newWeapon, GameObject gunBody)
    {
        bPlayerHasGun = true;
        if (_gunInventory[_currentlySelectedWeapon].gunObject)
        {
            _gunInventory[_currentlySelectedWeapon].gunObject.SetActive(false);
        }
        GunValues currentWeapon = newWeapon.ReturnGunValues();
        int weaponSlot = 0;
        switch (currentWeapon.SLOTTYPE)
        {
            case GunTypes.Primary: weaponSlot = 0; break;
            case GunTypes.Secondary: weaponSlot = 1; break;
            case GunTypes.Infinite: weaponSlot = 2; break;
            case GunTypes.Throwable: weaponSlot = 3; break;
        }
        _currentWeaponInterface = (IWeapon)gunBody.GetComponent(typeof(IWeapon));
        Destroy(_gunInventory[weaponSlot].gunObject);
        _gunInventory[weaponSlot].gunObject = gunBody;
        _currentlySelectedWeapon = weaponSlot;
        int _clipSize = _currentWeaponInterface.ReturnClipSize();
        _currentWeaponInterface.setCurrentClip(_clipSize);
        _attackScript.UpdateWeaponInterface(_currentWeaponInterface);
        float percentage = _currentWeaponInterface.ReturnClipSize() * playerStats.GetUpgradeLevelMultiplier(PlayerUpgradeType.reload);
        uiManagerScript.SetGauge(gameObject.tag, (int)percentage);
        uiManagerScript.SetCurrentAmmoText(gameObject.tag, _currentWeaponInterface.ReturnClipSize(),_gunInventory[weaponSlot].currentAmmo, _currentWeaponInterface.ReturnClipSize());
        PlaySwitchingSound();
        if (weaponSlot != 3)
        {
            uiManagerScript.SwitchGunImages(gameObject.tag, false, ThrowableType.FireGrenade, false, _currentWeaponInterface.RetrieveGunValues().ReturnGunValues().GUNTYPE, _currentWeaponInterface.RetrieveGunValues().ReturnGunValues().RARITY, _currentWeaponInterface.RetrieveGunValues().ReturnGunValues().EFFECT);
        }

        if (weaponSlot == 0 || weaponSlot == 1)
        {
            uiManagerScript.EnableWeaponSlot(gameObject.tag, _currentlySelectedWeapon);
        }

        uiManagerScript.SwitchSelectedWeaponUI(gameObject.tag, _currentlySelectedWeapon);
    }

    //function can be used to add ammo upon ammo pickup, or remove ammo when shooting etc.
    public void ChangeAmmo (int mAmount)
    {
        _gunInventory[_currentlySelectedWeapon].currentAmmo += mAmount;
        if (_gunInventory[_currentlySelectedWeapon].currentAmmo <= 0)
        {
            _gunInventory[_currentlySelectedWeapon].currentAmmo = 0;
        }
    }

    public bool AddAmmoToWeapon(int mAmount, WeaponType mType)
    { // Adds ammo to a specific weapon even when not active
        // will not take ammo if weapon isn't in inventory
        // returns true if ammo successfully taken

        for(int i = 0; i < _gunInventory.Length; i++)
        {
            if (_gunInventory[i].gunObject != null)
            {
                IWeapon gunInterface = _gunInventory[i].gunObject.GetComponent<IWeapon>();
                if (gunInterface != null && gunInterface.RetrieveGunValues().ReturnGunValues().GUNTYPE == mType)
                {
                    _gunInventory[i].currentAmmo += mAmount;
                    if (_currentlySelectedWeapon != 2 && _currentlySelectedWeapon != 3)
                    {
                        uiManagerScript.SetCurrentAmmoText(gameObject.tag, gunInterface.ReturnClipSize(), _gunInventory[_currentlySelectedWeapon].currentAmmo, gunInterface.ReturnClipSize());
                    }
                    return true;
                }
            }
        }

        return false;
    }

    //function to get the value of ammo for each weapon slot
    public int GetAmmo(int mAmmoSlot)
    {
        return _gunInventory[_currentlySelectedWeapon].currentAmmo;
    }

    void Reload()
    {
        if (_gunInventory[_currentlySelectedWeapon].gunObject && !_grenadeSelected && _currentlySelectedWeapon!=2)
        {

            if (_currentWeaponInterface.returnCurrentMagazineValue()==0)
            {
                RemoveCritBoost();
            }
           

            bool fullMagazine = _currentWeaponInterface.returnCurrentMagazineValue() == _currentWeaponInterface.ReturnClipSize();
            if (XCI.GetButtonDown(XboxButton.X, _inputScript.controller) && !fullMagazine && _gunInventory[_currentlySelectedWeapon].currentAmmo>0)
            {
                GetComponent<SCR_PlayerWorldSpaceUI>().ShowReloadPrompt(true);
                if (!_isReloading)
                {
                    if (bCritActive)
                    {
                        RemoveCritBoost();
                    }
                    critGauge = null;
                    _isReloading = true;
                    _currentWeaponInterface.GunReload(true);
                    normalGauge = uiManagerScript.DecreaseSteamGauge(gameObject.tag, this,reloadMultiplier, playerWorldSpaceUIScr);
                    _currentWeaponInterface.PlayReloadSound();
                    StartCoroutine(normalGauge);

                    if (!bHasReloaded && bTutorialCanReload)
                    {
                        GameObject tutorialManager = GameObject.FindGameObjectWithTag("TutorialManager");
                        if (tutorialManager)
                        {
                            SCR_TutorialManager tutorialManagerScr = tutorialManager.GetComponent<SCR_TutorialManager>();
                            tutorialManagerScr.SetHasReloaded();
                        }
                        bHasReloaded = true;
                    }

                    playerWorldSpaceUIScr.HideReloadButtonPrompt();

                }
                else if(_isReloading && !_CritAttempted)
                {
                    if (normalGauge != null)
                    {
                        StopCoroutine(normalGauge);
                        normalGauge = null;
                    }

                    critGauge = uiManagerScript.rotateGaugePinFull(0.0f, this, gameObject.tag,false);
                    StartCoroutine(critGauge);


                    playerWorldSpaceUIScr.ShowReloadPrompt(false);
                    //Original system
                    //float clipPercentage = uiManagerScript.ReturnPercentage(gameObject.tag);   
                    //float ammoGaugePercentage = RefillClip(clipPercentage);


                    //EGX SYSTEM
                    float ammoGaugePercentage = RefillClip(100);


                    _CritAttempted = true;
                    uiManagerScript.SetCurrentAmmoText(gameObject.tag, ammoGaugePercentage * _currentWeaponInterface.ReturnClipSize(), _gunInventory[_currentlySelectedWeapon].currentAmmo, _currentWeaponInterface.ReturnClipSize());                    

                    if (uiManagerScript.WithinCritZone())
                    {
                        CriticalDamageBonus();
                        SCR_AudioManager.instance.Play("DingReload");
                    }
                }
            }
        }     
    }

    float RefillClip(float mMultiplier)
    {
        int _clipSize = _currentWeaponInterface.ReturnClipSize();

        if (_clipSize > _gunInventory[_currentlySelectedWeapon].currentAmmo)
        {
            _clipSize = _gunInventory[_currentlySelectedWeapon].currentAmmo;
        }

        _clipSize = (int) ((float)_clipSize * mMultiplier);
        _clipSize = Mathf.Clamp(_clipSize, 1, _currentWeaponInterface.ReturnClipSize());
        _currentWeaponInterface.setCurrentClip(_clipSize);
        _gunInventory[_currentlySelectedWeapon].currentAmmo -= _clipSize;
            return ((float)_clipSize /(float) _currentWeaponInterface.ReturnClipSize());
    }

    void WeaponSwitching()
    {
        ///////////////////////////////////////////////////////// WEAPON SWITCHING //////////////////////////////////////////////
        int prevSelectedWeapon = _currentlySelectedWeapon;

        // DPAD input
        if (XCI.GetDPadDown(XboxDPad.Right, _inputScript.controller))
        {
            //switch to weapon slot 0
            prevSelectedWeapon = 0; //set new selected weapon slot enum
            switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Right;
            if (_gunInventory[prevSelectedWeapon].gunObject != null)
            {
                playerWorldSpaceUIScr.StartWeaponSwitchWorldSpaceUI(switchDirection);
            }
        }

        if (XCI.GetDPadDown(XboxDPad.Left, _inputScript.controller))
        {
            prevSelectedWeapon = 1;
            switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Left;
            if (_gunInventory[prevSelectedWeapon].gunObject != null)
            {
                playerWorldSpaceUIScr.StartWeaponSwitchWorldSpaceUI(switchDirection);
            }
        }

        if (XCI.GetDPadDown(XboxDPad.Up, _inputScript.controller))
        {
            prevSelectedWeapon = 2; //set new selected weapon slot enum
            switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Up;
            if (_gunInventory[prevSelectedWeapon].gunObject != null)
            {
                playerWorldSpaceUIScr.StartWeaponSwitchWorldSpaceUI(switchDirection);
            }
        }

        float leftTrigHeight = MAX_TRG_SCL * (1.0f - XCI.GetAxis(XboxAxis.LeftTrigger, _inputScript.controller));


        if (leftTrigHeight < 1.0f && !bLeftTriggerDown)
        {
            bLeftTriggerDown = true;
            grenadePreviousWeapon = prevSelectedWeapon;

            prevSelectedWeapon = 3; //set new selected weapon slot enum
            switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Down;
            if (_gunInventory[prevSelectedWeapon].gunObject != null)
            {
                playerWorldSpaceUIScr.StartWeaponSwitchWorldSpaceUI(switchDirection);
                _gunInventory[prevSelectedWeapon].gunObject.GetComponent<SCR_GrenadeParent>().GrenadePossible();
            }
        }

        if (leftTrigHeight >= 1.0f && bLeftTriggerDown)
        {
            bLeftTriggerDown = false;
            prevSelectedWeapon = grenadePreviousWeapon;
            switch (prevSelectedWeapon)
            {
                case 0: switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Right; break;
                case 1: switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Left; break;
                case 2: switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Up; break;
            }

            if (_gunInventory[prevSelectedWeapon].gunObject != null)
            {
                playerWorldSpaceUIScr.StartWeaponSwitchWorldSpaceUI(switchDirection);
                _gunInventory[prevSelectedWeapon].gunObject.GetComponent<SCR_AutomaticWeapon>().SetBulletTimer(0.5f);
            }
        }

        if (XCI.GetButtonUp(XboxButton.Y, _inputScript.controller))
        {
            //loop through inventory until next gunobject is found
            prevSelectedWeapon = CycleWeapons(prevSelectedWeapon);

            //set worldspace UI elements of new gun object
            switch (prevSelectedWeapon)
            {
                case 0:
                    switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Right;
                    break;
                case 1:
                    switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Left;
                    break;
                case 2:
                    switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Up;
                    break;
                case 3:
                    switchDirection = SCR_PlayerWorldSpaceUI.WeaponSwitchDirection.Down;
                    break;
            }

            if (_gunInventory[prevSelectedWeapon].gunObject != null)
            {
                playerWorldSpaceUIScr.StartWeaponSwitchWorldSpaceUI(switchDirection);
            }
        }

        if (prevSelectedWeapon != _currentlySelectedWeapon)
        {
            if (_gunInventory[prevSelectedWeapon].gunObject!=null)
            {

                StopReloadCoroutines();
                uiManagerScript.ResetGaugePin(gameObject.tag);
                if (_gunInventory[_currentlySelectedWeapon].gunObject != null)
                {
                    ResetReloadValues();
                    _gunInventory[_currentlySelectedWeapon].gunObject.SetActive(false);
                }

                // remove reload prompt if they switch weapon to a weapon with ammo
                if (GetAmmo(_currentlySelectedWeapon) > 0)
                {
                    if (playerWorldSpaceUIScr.bIsDisplayed)
                    {
                        playerWorldSpaceUIScr.ShowReloadPrompt(false);
                    }
                }
                else
                {
                    if (!playerWorldSpaceUIScr.bIsDisplayed)
                    {
                        playerWorldSpaceUIScr.ShowReloadPrompt(true);
                    }
                }

                if (!bHasSwitchedWeapons && bTutorialCanSwitchWeapons)
                {
                    GameObject tutorialManager = GameObject.FindGameObjectWithTag("TutorialManager");
                    if (tutorialManager)
                    {
                        SCR_TutorialManager tutorialManagerScr = tutorialManager.GetComponent<SCR_TutorialManager>();
                        tutorialManagerScr.SetHasSwitchedWeapons();
                    }
                    bHasSwitchedWeapons = true;
                }

                if (prevSelectedWeapon!=3)
                {
                    _grenadeSelected = false;
                    _currentlySelectedWeapon = prevSelectedWeapon;
                    _attackScript.setGrenadeSelected = _grenadeSelected;
                    _attackScript.setGrenadeSCR = null;
                    _currentWeaponInterface = (IWeapon)_gunInventory[prevSelectedWeapon].gunObject.GetComponent(typeof(IWeapon));
                    _gunInventory[prevSelectedWeapon].gunObject.SetActive(true);                    
                    _attackScript.UpdateWeaponInterface(_currentWeaponInterface);
                    if (_currentlySelectedWeapon != 2)
                    {
                        float percentage = _currentWeaponInterface.ReturnClipSize() * playerStats.GetUpgradeLevelMultiplier(PlayerUpgradeType.reload);
                        uiManagerScript.SetGauge(gameObject.tag,(int)percentage);
                        float clipPercentage = _currentWeaponInterface.ClipPercentagerFilled();
                        uiManagerScript.SetCurrentAmmoText(gameObject.tag, clipPercentage * _currentWeaponInterface.ReturnClipSize(), _gunInventory[prevSelectedWeapon].currentAmmo, _currentWeaponInterface.ReturnClipSize());
                        uiManagerScript.SwitchGunImages(gameObject.tag, false, ThrowableType.FireGrenade, false, _currentWeaponInterface.RetrieveGunValues().ReturnGunValues().GUNTYPE, _currentWeaponInterface.RetrieveGunValues().ReturnGunValues().RARITY, _currentWeaponInterface.RetrieveGunValues().ReturnGunValues().EFFECT);
                    }
                    else
                    {
                        uiManagerScript.SetGauge(gameObject.tag, 0);
                        float clipPercentage = _currentWeaponInterface.ClipPercentagerFilled();
                        uiManagerScript.SetCurrentAmmoText(gameObject.tag, clipPercentage * _currentWeaponInterface.ReturnClipSize(), _gunInventory[prevSelectedWeapon].currentAmmo, _currentWeaponInterface.ReturnClipSize(), true);
                        uiManagerScript.SwitchGunImages(gameObject.tag, false, ThrowableType.FireGrenade, true);
                    }


                    PlaySwitchingSound();


                }
                else
                {
                    _currentWeaponInterface = null;
                    _currentlySelectedWeapon = prevSelectedWeapon;
                    _grenadeSelected = true;
                    SCR_GrenadeParent currentGrenadeScript = _gunInventory[_currentlySelectedWeapon].gunObject.GetComponent<SCR_GrenadeParent>();
                    _attackScript.setGrenadeSCR = currentGrenadeScript;
                    currentGrenadeScript.bHasResetVelocity = false;
                    _attackScript.setGrenadeSelected = _grenadeSelected;
                    _gunInventory[_currentlySelectedWeapon].gunObject.SetActive(true);



                    if (_gunInventory[_currentlySelectedWeapon].currentAmmo > 0)
                    {
                        currentGrenadeScript.setCurrentAmmo(_gunInventory[_currentlySelectedWeapon].currentAmmo);
                        //_gunInventory[_currentlySelectedWeapon].currentAmmo = 0;

                    }
                    uiManagerScript.SetCurrentAmmoText(gameObject.tag, _gunInventory[_currentlySelectedWeapon].currentAmmo, 0, 0);
                    uiManagerScript.SetGauge(gameObject.tag, 0);

                    if (currentGrenadeScript)
                    {
                        uiManagerScript.SwitchGunImages(gameObject.tag, true, currentGrenadeScript.GetGrenadeType());
                    }
                    
                    weaponSwitchingSource.PlayOneShot(grenadeSwitchingClip);
                }

                uiManagerScript.SwitchSelectedWeaponUI(gameObject.tag, prevSelectedWeapon);

                //playerWorldSpaceUIScr.StartWeaponSwitchWorldSpaceUI(switchDirection);

            }
            else
            {
                weaponSwitchingSource.PlayOneShot(failedWeaponSwitchClip);
            }
        }
    }

    int CycleWeapons(int mPrevSelectedWeapon)
    {
        int startSelectedWeapon = mPrevSelectedWeapon;
        int currentSelectedWeapon = mPrevSelectedWeapon;
        if (currentSelectedWeapon < 2)
        {
            currentSelectedWeapon++;
        }
        else
        {
            currentSelectedWeapon = 0;
        }
        while (_gunInventory[currentSelectedWeapon].gunObject == null)
        {
            if (currentSelectedWeapon == startSelectedWeapon)
            {
                return startSelectedWeapon;
            }

            if (currentSelectedWeapon < 3)
            {
                currentSelectedWeapon++;
            }
            else
            {
                currentSelectedWeapon = 0;
            }
        }
        return currentSelectedWeapon;
    }

    public void SetCurrentWeaponClass()
    {
        if (_gunInventory[uiManagerScript.generatedGunWeaponSlot].gunObject != null)
        {
            IWeapon uiManagerCurrentWeapon = (IWeapon)_gunInventory[uiManagerScript.generatedGunWeaponSlot].gunObject.GetComponent(typeof(IWeapon));
            uiManagerScript.currentWeaponClass = uiManagerCurrentWeapon.RetrieveGunValues();
        }
        else
        {
            uiManagerScript.currentWeaponClass = null;
        }
    }

    void PlaySwitchingSound()
    {
        AudioClip next = weaponSwitchingClips[Random.Range(0, weaponSwitchingClips.Length)];
        weaponSwitchingSource.PlayOneShot(next);
    }

    void StopReloadCoroutines()
    {
        if (normalGauge != null)
        {
            StopCoroutine(normalGauge);
            normalGauge = null;
        }

        if (critGauge != null)
        {
            StopCoroutine(critGauge);
            critGauge = null;
        }

        if (critUpgrade != null)
        {
            StopCoroutine(critUpgrade);
            critUpgrade = null;
            RemoveCritBoost();
        }

        if (ammoGauge != null)
        {
            StopCoroutine(ammoGauge);
            ammoGauge = null;
        }
    }

    public void StartGaugePinReturn()
    {
        if (normalGauge != null)
        {
            StopCoroutine(normalGauge);
            normalGauge = null;
        }
        normalGauge = null;
        critGauge = uiManagerScript.rotateGaugePinFull(0.0f, this, gameObject.tag, true);
        StartCoroutine(critGauge);
    }

    public void ReloadComplete()
    {

        int currentGunClipSize = _currentWeaponInterface.ReturnClipSize();
        float clipPercentage = 1.0f;
        if (_gunInventory[_currentlySelectedWeapon].currentAmmo < currentGunClipSize)
        {
            clipPercentage =(float) _gunInventory[_currentlySelectedWeapon].currentAmmo / (float)currentGunClipSize;
        }       
        RefillClip(1.0f);
        ResetReloadValues();

        uiManagerScript.SetCurrentAmmoText(gameObject.tag, clipPercentage * _currentWeaponInterface.ReturnClipSize(), _gunInventory[_currentlySelectedWeapon].currentAmmo, _currentWeaponInterface.ReturnClipSize());

    }
    public void ResetReloadValues()
    {
        _isReloading = false;
        _CritAttempted = false;
        if (_currentWeaponInterface!=null)
        {
            _currentWeaponInterface.GunReload(false);
        }
        
    }

    void RemoveCritBoost()
    {
        bCritActive = false;
        uiManagerScript.SetIndicatorActive(gameObject.tag, false);
        _currentWeaponInterface.SetDamageBonus(1.0f);
        playerWorldSpaceUIScr.DestroyCritParticle();
    }

    void CriticalDamageBonus()
    {
        bCritActive = true;
        _currentWeaponInterface.SetDamageBonus(critDamageMultiplier);
        uiManagerScript.SetIndicatorActive(gameObject.tag, true);
        playerWorldSpaceUIScr.CreateCritParticle();
    }

    public void AdjustCriticalZoneAffectAfterReloadUpgrade()
    {
        float percentage = _currentWeaponInterface.ReturnClipSize() * playerStats.GetUpgradeLevelMultiplier(PlayerUpgradeType.reload);
        uiManagerScript.SetGauge(gameObject.tag, (int)percentage);
    }
}

[System.Serializable]
public struct STR_GunInventory
{
    public GameObject gunObject;
    public int currentAmmo;
}