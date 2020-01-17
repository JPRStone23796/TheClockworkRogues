using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
[CreateAssetMenu(fileName = "Abilities", menuName = "Abilities/Heal")]
public class SCR_HealAbility : SCR_Ability
{
    private SCR_PlayerHealth playerHealthSCR;


    [SerializeField] private float healthGainedPerSecond = 1.0f;
    public override void CarryOutAbility()
    {
        playerHealthSCR.GiveHealth((healthGainedPerSecond * Time.deltaTime));
    }


    public override void SetPlayerParent(GameObject mPlayer)
    {
        playerHealthSCR = mPlayer.GetComponent<SCR_PlayerHealth>();
    }
}
