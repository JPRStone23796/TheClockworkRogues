using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class SCR_AutomaticWeapon : MonoBehaviour,IWeapon
{
    [SerializeField]
    [Tooltip("Displays the structure of the weapon, such as its components as well as its damage values etc")]
    private SCR_GunClass _weaponStructure;
    [Header("Position of where the muzzle flash will be spawned")]
    [SerializeField] Transform _gunPosition;
    private GunValues _gunValues;

    private const float MAX_BUL_TME = 0.3f;
    [SerializeField] private float bulletTimer = MAX_BUL_TME;
    [Space(20)]
    [SerializeField]
    private int _currentClip = 0;

    private bool isReloading = false;

    [Header("Weapons Current Ability")]
    [SerializeField]
    [Space(20)]
    private SCR_Ability gunAbility = null;

    private GameObject _playerParent;
    private SCR_PlayerAttack playerAttackScr;
    private SCR_PlayerWorldSpaceUI worldSpaceUIScript;

    private float _damageMultiplier = 1.0f;

    [SerializeField] GameObject muzzle, trail, hitEffect, particleEffectController;

    [Header("Spawn Positions for Gun Components")]
    GameObject currentMuzzle;


    [SerializeField] private GameObject scopePosition, clipPosition, underBarrelPosition, barrelPosition;

    [Space(20)]
    [Tooltip("Will determine in script where the gun will have infinite ammo")]
    [SerializeField] private bool IsInfinitePistol = false;
   


   
    [Tooltip("Will determine the minimum and maxium angle spread for aim assist")]
    [SerializeField] private int angleMin = -10, angleMax = 10;



    [Header("Audio Systems")]
    [Space(20)]
    [SerializeField] private AudioClip[] shotSoundClips;

    [SerializeField] private AudioClip[] reloadSoundClips;

    [SerializeField] private AudioClip[] impactClips, enemyImpactClips;

    [Tooltip("Audio sources for both the fire sound clips, as well as the impact sound effects")]
    [Space(20)]
    [SerializeField] private AudioSource currentGunShotSource;

    [Tooltip("Sound clip that will play with an empty magazine")]
    [Space(20)]
    [SerializeField] private AudioClip emptyMagazineClip;


    [Tooltip("Vibrations values used when the gun is fired, Rumble ranges from 0 to 1 and the timer will determine how long the rumble will last for")]
    [Space(30)]
    [SerializeField] private float controllerRumbleValue, vibrationTimer;

    [Tooltip("Layermasks being used by the weapon")]
    [Space(20)]
    [SerializeField] private LayerMask hitDetectionLayerMask;



    [Tooltip("Scale value of the muzzle flash")]
    [Space(20)]
    [Range(0.1f,1.0f)]
    [SerializeField] private float muzzleFlashScale = 0.1f;

    private float lowerPitchValue = 0.72f, higherPitchValue = 1.28f;

    void Awake()
    {
        if (!IsInfinitePistol)
        {
            _weaponStructure = new SCR_GunClass();
            _gunValues = new GunValues();
        }
        _playerParent = null;   
        if (IsInfinitePistol)
        {
            _gunValues = _weaponStructure.ReturnGunValues();
            _currentClip = _gunValues.CLIPSIZE;
        }

        currentGunShotSource = GetComponent<AudioSource>();
        currentMuzzle = Instantiate(muzzle, _gunPosition.position, Quaternion.identity);
        currentMuzzle.transform.parent = _gunPosition.transform;
        currentMuzzle.transform.localRotation = new Quaternion(0,0,0,0);
        currentMuzzle.transform.localScale = new Vector3(muzzleFlashScale, muzzleFlashScale, muzzleFlashScale);
        currentMuzzle.SetActive(false);

    }

    void Update()
    {
        
        bulletTimer -= Time.deltaTime;
        if (gunAbility != null)
        {
            gunAbility.CarryOutAbility();
        }
    }


    public void SetAmmoLimitToInfinite()
    {
        IsInfinitePistol = true;
    }


    public Transform ReturnGunEnd()
    {
        return _gunPosition;
    }


    public float ReturnControllerRumbleValue()
    {
        return controllerRumbleValue;
    }

    public float ReturnRumbleTime()
    {
        return vibrationTimer;
    }

    public void PlayReloadSound()
    {
        AudioClip currentReload = reloadSoundClips[Random.Range(0, reloadSoundClips.Length)];
        currentGunShotSource.pitch = 1;
        currentGunShotSource.PlayOneShot(currentReload, currentGunShotSource.volume);
    }

    AudioClip playImpactSound(bool enemy)
    {

        AudioClip currentImpact;

        if (enemy)
        {
            currentImpact = enemyImpactClips[Random.Range(0, enemyImpactClips.Length)];
        }
        else
        {
            currentImpact = impactClips[Random.Range(0, impactClips.Length)];
        }
        return currentImpact;


    }

    public void SetBulletTimer(float mValue)
    {
        bulletTimer = mValue;
    }

    public bool Fire()
    {
        bool bulletFired = false;
        
        if (bulletTimer <= 0 && _currentClip>0 && !isReloading)
        {
           
            RaycastHit hitCamera;

            Vector3 pointTowards = new Vector3();
            pointTowards = transform.position + (transform.right * 9999);
            Vector3 direction = _gunPosition.position + transform.right;
            direction.y = transform.position.y;
            direction = direction - transform.position;



            if (Physics.Raycast(transform.position, direction.normalized, out hitCamera, Mathf.Infinity, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
            {
                pointTowards = hitCamera.point;
            }
            
            pointTowards = GetShotPosAfterSpread(pointTowards, _gunValues.ACCURACY);

            Shoot(pointTowards);

            if (!IsInfinitePistol)
            {
                _currentClip--;
            }

            if (_playerParent)
            {
                if (!IsInfinitePistol)
                {
                    SCR_UIManager.instance.SetCurrentAmmoText(_playerParent.tag, _currentClip, -1, _gunValues.CLIPSIZE);
                }
                else
                {
                    SCR_UIManager.instance.SetCurrentAmmoText(_playerParent.tag, _currentClip, -1, _gunValues.CLIPSIZE, true);
                }
            }
           
            bulletTimer = (1 / (_gunValues.RATEOFFIRE)) * 5.0f;
            currentMuzzle.SetActive(true);

            //currentMuzzle.transform.LookAt(pointTowards);
            //currentMuzzle.transform.Rotate(Vector3.up, -90.0f);

            Invoke("DeactivateMuzzleEffect", 0.1f);
            currentGunShotSource.pitch = Random.Range(lowerPitchValue, higherPitchValue);
            currentGunShotSource.PlayOneShot(RetreiveGunShot(), currentGunShotSource.volume);
            bulletFired = true;
        }

        if (bulletTimer <= 0 && _currentClip <= 0 && !isReloading)
        {
            int weaponSlot = 0;
            switch (_gunValues.SLOTTYPE)
            {
                case GunTypes.Primary: weaponSlot = 0; break;
                case GunTypes.Secondary: weaponSlot = 1; break;
                case GunTypes.Infinite: weaponSlot = 2; break;
                case GunTypes.Throwable: weaponSlot = 3; break;
            }

            if (!worldSpaceUIScript)
            {
                if(_playerParent)
                {
                    worldSpaceUIScript = _playerParent.GetComponent<SCR_PlayerWorldSpaceUI>();
                }
              
            }
            if (worldSpaceUIScript)
            {
                if (_playerParent.GetComponent<SCR_CharacterInventory>().GetAmmo(weaponSlot) > 0)
                {
                    if (!worldSpaceUIScript.bIsDisplayed)
                    {
                        worldSpaceUIScript.ShowReloadPrompt(true);
                    }
                }
            }
        }

        if (bulletTimer <= 0 &&_currentClip <= 0 && !bulletFired)
        {
            currentGunShotSource.pitch = 1;
            currentGunShotSource.PlayOneShot(emptyMagazineClip);
            bulletTimer = (1 / (_gunValues.RATEOFFIRE)) * 5.0f;
        }

        return bulletFired;
    }

   ///Required for an Invoke statement
    void DeactivateMuzzleEffect()
    {
        currentMuzzle.SetActive(false);
    }

    AudioClip RetreiveGunShot()
    {
        AudioClip currentGunShot = shotSoundClips[Random.Range(0, shotSoundClips.Length)];
        return currentGunShot;
    }

   

    Vector3 GetShotPosAfterSpread(Vector3 pointTowards, float spread)
    {
        spread *= Mathf.Abs(Vector3.Distance(transform.position, pointTowards));
        Vector3 randPos = Random.insideUnitCircle * (1 / spread);
        return pointTowards + randPos;
    }


    void Shoot(Vector3 pointTowards)
    {
        Ray currentShot;
        RaycastHit hit;

        bool enemyHit = false;
        currentShot = new Ray(_gunPosition.position, (pointTowards - transform.position));
        float distance = Vector3.Distance(pointTowards, transform.position);
        Vector3 direction = (pointTowards - _gunPosition.position).normalized;
        for (int i=angleMin;i<=angleMax;i++)
        {
            Vector3 potentialDir = Quaternion.AngleAxis(i, Vector3.up) * direction;
            if (Physics.Raycast(_gunPosition.position, potentialDir, out hit, distance, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 finalPos = hit.point;
                if (hit.transform.CompareTag("Enemy"))
                {
                    IDamageable currentDamageableInterface = (IDamageable)hit.transform.gameObject.GetComponent(typeof(IDamageable));
                    float damage = _gunValues.DPS;
                    damage *= _damageMultiplier;
                    currentDamageableInterface.DamageEnemy(damage);
                    finalPos.y = hit.transform.position.y;
                    enemyHit = true;

                    MoveBullet(finalPos,enemyHit);
                 
                    if (_gunValues.GUNTYPE == WeaponType.shotgun)
                    {
                        ShotgunShot(i, direction, distance);
                    }

                    SCR_GameManager.instance.PlayerDealtDamage(transform.root.tag, damage);

                    break;
                  
                }
            }        
        }

        if (enemyHit == false)
        {
            if (Physics.Raycast(currentShot, out hit, Mathf.Infinity, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 finalPos = hit.point;
                if (hit.transform.CompareTag("Enemy"))
                {
                    IDamageable currentDamageableInterface = (IDamageable)hit.transform.gameObject.GetComponent(typeof(IDamageable));
                    float damage = _gunValues.DPS;
                    damage *= _damageMultiplier;
                    currentDamageableInterface.DamageEnemy(damage);
                    finalPos.y = hit.transform.position.y;
                    enemyHit = true;
                }
                MoveBullet(finalPos,enemyHit);

                if (_gunValues.GUNTYPE == WeaponType.shotgun)
                {
                    ShotgunShotMiss(currentShot.direction);
                }
            }
        }


        // Draws line in the editor for debugging, can enable gizmos in game view to see there
        //Debug.DrawRay(_gunPosition.position, pointTowards - _gunPosition.position, Color.black, 10f); //, 2.0f);
    }

    void ShotgunShot(int i, Vector3 mDirection, float mDistance)
    {
        //MISSED BULLETS DON'T CURRENTLY CONTINUE ON PAST ENEMY HIT POINTS. WILL FIX IF NOTICEABLE
        RaycastHit myHit;
        bool enemyHit = false;
        // bullet to right of inital hit
        Vector3 potentialDir = Quaternion.AngleAxis(i + 5, Vector3.up) * mDirection;
        if (Physics.Raycast(_gunPosition.position, potentialDir, out myHit, mDistance, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 myFinalPos = myHit.point;

            if (myHit.transform.CompareTag("Enemy"))
            {
                IDamageable currentDamageableInterface = (IDamageable)myHit.transform.gameObject.GetComponent(typeof(IDamageable));
                float damage = _gunValues.DPS;
                damage *= _damageMultiplier;
                currentDamageableInterface.DamageEnemy(damage);
                myFinalPos.y = myHit.transform.position.y;
                MoveBullet(myFinalPos,enemyHit);
                enemyHit = true;
            }
            else
            {
                if (Physics.Raycast(_gunPosition.position, potentialDir, out myHit, Mathf.Infinity, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3 otherFinalPos = myHit.point;
                    if (myHit.transform.CompareTag("Enemy"))
                    {
                        IDamageable currentDamageableInterface = (IDamageable)myHit.transform.gameObject.GetComponent(typeof(IDamageable));
                        float damage = _gunValues.DPS;
                        damage *= _damageMultiplier;
                        currentDamageableInterface.DamageEnemy(damage);
                        otherFinalPos.y = myHit.transform.position.y;
                        enemyHit = true;
                    }
                    MoveBullet(otherFinalPos,enemyHit);
                }
            }
        }

        // bullet to left of inital hit
        Vector3 otherPotentialDir = Quaternion.AngleAxis(i - 5, Vector3.up) * mDirection;
        if (Physics.Raycast(_gunPosition.position, otherPotentialDir, out myHit, mDistance, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 myFinalPos = myHit.point;

            if (myHit.transform.CompareTag("Enemy"))
            {
                IDamageable currentDamageableInterface = (IDamageable)myHit.transform.gameObject.GetComponent(typeof(IDamageable));
                float damage = _gunValues.DPS;
                damage *= _damageMultiplier;
                currentDamageableInterface.DamageEnemy(damage);
                myFinalPos.y = myHit.transform.position.y;
                enemyHit = true;
                MoveBullet(myFinalPos,enemyHit);
            }
            else
            {
                if (Physics.Raycast(_gunPosition.position, otherPotentialDir, out myHit, Mathf.Infinity, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3 otherFinalPos = myHit.point;
                    if (myHit.transform.CompareTag("Enemy"))
                    {
                        IDamageable currentDamageableInterface = (IDamageable)myHit.transform.gameObject.GetComponent(typeof(IDamageable));
                        float damage = _gunValues.DPS;
                        damage *= _damageMultiplier;
                        currentDamageableInterface.DamageEnemy(damage);
                        otherFinalPos.y = myHit.transform.position.y;
                        enemyHit = true;
                    }
                    MoveBullet(otherFinalPos, enemyHit);
                }
            }
        }


    }

    void ShotgunShotMiss(Vector3 mDirection)
    {
        //MISSED BULLETS DON'T CURRENTLY CONTINUE ON PAST ENEMY HIT POINTS. WILL FIX IF NOTICEABLE
        RaycastHit myHit;
        Vector3 myDirectionRight = Quaternion.AngleAxis(5, Vector3.up) * mDirection;
        Vector3 myDirectionLeft = Quaternion.AngleAxis(-5, Vector3.up) * mDirection;
        bool enemyHit = false;
        // bullet to right of inital hit
        if (Physics.Raycast(_gunPosition.position, myDirectionRight, out myHit, Mathf.Infinity, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 myFinalPos = myHit.point;

            if (myHit.transform.CompareTag("Enemy"))
            {
                IDamageable currentDamageableInterface = (IDamageable)myHit.transform.gameObject.GetComponent(typeof(IDamageable));
                float damage = _gunValues.DPS;
                damage *= _damageMultiplier;
                currentDamageableInterface.DamageEnemy(damage);
                myFinalPos.y = myHit.transform.position.y;
                enemyHit = true;
                MoveBullet(myFinalPos,enemyHit);
            }
            else
            {
                if (Physics.Raycast(_gunPosition.position, myDirectionRight, out myHit, Mathf.Infinity, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3 otherFinalPos = myHit.point;
                    if (myHit.transform.CompareTag("Enemy"))
                    {
                        IDamageable currentDamageableInterface = (IDamageable)myHit.transform.gameObject.GetComponent(typeof(IDamageable));
                        float damage = _gunValues.DPS;
                        damage *= _damageMultiplier;
                        currentDamageableInterface.DamageEnemy(damage);
                        otherFinalPos.y = myHit.transform.position.y;
                        enemyHit = true;
                    }
                    MoveBullet(otherFinalPos, enemyHit);
                }
            }
        }

        // bullet to left of inital hit
        if (Physics.Raycast(_gunPosition.position, myDirectionLeft, out myHit, Mathf.Infinity, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 myFinalPos = myHit.point;

            if (myHit.transform.CompareTag("Enemy"))
            {
                IDamageable currentDamageableInterface = (IDamageable)myHit.transform.gameObject.GetComponent(typeof(IDamageable));
                float damage = _gunValues.DPS;
                damage *= _damageMultiplier;
                currentDamageableInterface.DamageEnemy(damage);
                myFinalPos.y = myHit.transform.position.y;
                enemyHit = true;
                MoveBullet(myFinalPos, enemyHit);
            }
            else
            {
                if (Physics.Raycast(_gunPosition.position, myDirectionLeft, out myHit, Mathf.Infinity, hitDetectionLayerMask, QueryTriggerInteraction.Ignore))
                {
                    Vector3 otherFinalPos = myHit.point;
                    if (myHit.transform.CompareTag("Enemy"))
                    {
                        IDamageable currentDamageableInterface = (IDamageable)myHit.transform.gameObject.GetComponent(typeof(IDamageable));
                        float damage = _gunValues.DPS;
                        damage *= _damageMultiplier;
                        currentDamageableInterface.DamageEnemy(damage);
                        otherFinalPos.y = myHit.transform.position.y;
                        enemyHit = true;
                    }
                    MoveBullet(otherFinalPos, enemyHit);
                }
            }
        }

    }


 



    void MoveBullet(Vector3 hitPoint, bool enemyHit)
    {
        GameObject currentParticleEffectController = Instantiate(particleEffectController, transform.position, Quaternion.identity);

        GameObject currentTrail = Instantiate(trail, currentMuzzle.transform.position, Quaternion.identity);
        currentTrail.transform.localScale = new Vector3(muzzleFlashScale, muzzleFlashScale, muzzleFlashScale);
        currentTrail.transform.LookAt(hitPoint);
        currentTrail.transform.parent = currentParticleEffectController.transform;
        Vector3 startPos = currentMuzzle.transform.position + currentTrail.transform.forward * 2.0f;
        AudioClip currentClip = playImpactSound(enemyHit);
        float clipPitch = Random.Range(lowerPitchValue,higherPitchValue);
        SCR_BulletMovement currentMovementSCR = currentParticleEffectController.GetComponent<SCR_BulletMovement>();
        currentMovementSCR.StartMovement(hitPoint, startPos, currentTrail, hitEffect, currentClip, clipPitch);
    }





    public void SetGunValues(SCR_GunClass mWeaponValues, GameObject mplayerObj, SCR_Ability mWeaponAbility)
    {
        _weaponStructure = mWeaponValues;
        _gunValues = _weaponStructure.ReturnGunValues();
        _playerParent = mplayerObj;
        gunAbility = mWeaponAbility;
    }

    public SCR_GunClass RetrieveGunValues()
    {
        return _weaponStructure;
    }



    public GunPositions returnPositions()
    {
        GunPositions currentGuns = new GunPositions();
        currentGuns.scopePosition = scopePosition;
        currentGuns.barrelPosition = barrelPosition;
        currentGuns.clipPosition = clipPosition;
        currentGuns.underBarrelPosition = underBarrelPosition;
        return currentGuns;
    }


    public void setCurrentClip(int _clipIncrease)
    {
        _currentClip = _clipIncrease;
    }

    public int ReturnClipSize()
    {
        return _gunValues.CLIPSIZE;
    }


    public bool IsMagazineFull()
    {
        bool _magazineFull = _currentClip == _gunValues.CLIPSIZE;
        return _magazineFull;
    }



     public void GunReload(bool mReloading)
    {
        isReloading = mReloading;
    }

    public float ClipPercentagerFilled()
    {
        return ( (float)_currentClip / _gunValues.CLIPSIZE);
    }

    public void SetDamageBonus(float mMultiplier)
    {
        _damageMultiplier = mMultiplier;
    }

    public int returnCurrentMagazineValue()
    {
        return _currentClip;
    }


    public void SetParent(GameObject mParent)
    {
        _playerParent = mParent;
    }

}
