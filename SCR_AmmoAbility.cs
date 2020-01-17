using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Abilities", menuName = "Abilities/Ammo")]
public class SCR_AmmoAbility : SCR_Ability
{

	  private SCR_CharacterInventory playerInventorySCR;


    [SerializeField] private float TimeToGain1Ammo = 1.0f;
    private float currentTimer = 0.0f;

    public override void CarryOutAbility()
    {
        currentTimer += Time.deltaTime;
        if (currentTimer >= TimeToGain1Ammo)
        {
            playerInventorySCR.ChangeAmmo(1);
            currentTimer = 0.0f;
        }
    }


    public override void SetPlayerParent(GameObject mPlayer)
    {
        playerInventorySCR = mPlayer.GetComponent<SCR_CharacterInventory>();
    }
}
