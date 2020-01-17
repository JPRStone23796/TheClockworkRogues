using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XboxCtrlrInput;

public class SCR_TestPlayerAttack : MonoBehaviour
{

    public XboxController controller;

    private GameObject thrownBullet;

    private SCR_UIManager uiManagerScript;


    private const float MAX_TRG_SCL = 1.21f;

    private SCR_LaunchArcMesh launchScript;
    private Throwable myThrowable;


    private IWeapon _currentWeaponInterface;
    private SCR_GrenadeParent currentGrenade = null;
    private bool _weaponSelected = false;

    public SCR_TestPlayerAttack()
    {
        setGrenadeSelected = false;
    }


    public bool setGrenadeSelected { get; set; }

    public SCR_GrenadeParent setGrenadeSCR
    {
        set { currentGrenade = value; }
    }

    void Start()
    {
        controller = gameObject.GetComponent<SCR_PlayerInput>().controller;
    }

    // Update
    void Update()
    {

        float leftTrigHeight = MAX_TRG_SCL * (1.0f - XCI.GetAxis(XboxAxis.LeftTrigger, controller));
        float rightTrigHeight = MAX_TRG_SCL * (1.0f - XCI.GetAxis(XboxAxis.RightTrigger, controller));

        if (setGrenadeSelected)
        {
            bool aiming = leftTrigHeight < 1.0f;
            currentGrenade.Aim(aiming);
        }

        if (rightTrigHeight < 1.0f)
        {
            if (_weaponSelected && !setGrenadeSelected)
            {
                _currentWeaponInterface.Fire();
            }   
            else if (setGrenadeSelected)
            {
                currentGrenade.Fire();
            }
        }

    }




    public void UpdateWeaponInterface(IWeapon mWeapon)
    {
        _currentWeaponInterface = mWeapon;
        if (!_weaponSelected)
        {
            _weaponSelected = true;
        }
    }







}
