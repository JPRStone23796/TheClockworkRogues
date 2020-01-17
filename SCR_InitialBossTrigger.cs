using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_InitialBossTrigger : MonoBehaviour
{
    [SerializeField] private SCR_Boss bossScript;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player1" || other.tag == "Player2")
        {
            if (!bossScript.bossAttacking)
            {
                StartCoroutine(SCR_UIManager.instance.HideFightThroughStreetsPrompt());
                bossScript.BossAttack(true);
            }        
        }
    }



}
