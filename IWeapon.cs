using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWeapon
{

    bool Fire();

    void SetGunValues(SCR_GunClass mWeaponValues,GameObject mplayerObj, SCR_Ability mWeaponAbility);

    SCR_GunClass RetrieveGunValues();

    void setCurrentClip(int _clipIncrease);

    int ReturnClipSize();

    bool IsMagazineFull();

    void GunReload(bool mReloading);

    float ClipPercentagerFilled();

    void SetDamageBonus(float mMultiplier);

    int returnCurrentMagazineValue();


    GunPositions returnPositions();

    void SetParent(GameObject mParent);

    void PlayReloadSound();


    float ReturnControllerRumbleValue();

    float ReturnRumbleTime();

    Transform ReturnGunEnd();


    void SetAmmoLimitToInfinite();





}
