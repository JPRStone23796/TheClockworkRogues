using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_AssignBuffs : MonoBehaviour
{
    [SerializeField] private SCR_BuffType[] ListOfBuffs;
    private STR_CurrentPlayerBuffs player1Buffs, player2Buffs;
    private SCR_CharacterInventory player1Inventory, player2Inventory;


 

    public void AssignPlayerBuffs()
    {       
        player1Buffs = DeterminePlayerBuffs();
        player2Buffs = DeterminePlayerBuffs();
    }

    public STR_CurrentPlayerBuffs ReturnPlayer1Buffs()
    {
        return player1Buffs;
    }


    public STR_CurrentPlayerBuffs ReturnPlayer2Buffs()
    {
        return player2Buffs;
    }

    /// <summary>
    /// Called by the main menu manager before it destroys itself. The buffs that were determined at the main menu are passed in and assigned.
    /// </summary>
    /// <param name="mPlayer1Buffs"></param>
    /// <param name="mPlayer2Buffs"></param>
    public void SetPlayerBuffs(STR_CurrentPlayerBuffs mPlayer1Buffs, STR_CurrentPlayerBuffs mPlayer2Buffs)
    {
        player1Inventory = GameObject.FindGameObjectWithTag("Player1").GetComponent<SCR_CharacterInventory>();
        player2Inventory = GameObject.FindGameObjectWithTag("Player2").GetComponent<SCR_CharacterInventory>();

        player1Buffs = mPlayer1Buffs;
        player2Buffs = mPlayer2Buffs;

        if (player1Inventory)
            player1Inventory.SetPlayerBuff(mPlayer1Buffs);

        if (player2Inventory)
            player2Inventory.SetPlayerBuff(mPlayer2Buffs);
    }


    private STR_CurrentPlayerBuffs DeterminePlayerBuffs()
    {
        bool selected = false;

        STR_CurrentPlayerBuffs currentPlayerBuffs = new STR_CurrentPlayerBuffs();
        SCR_BuffType selectedBuff = new SCR_BuffType();
        currentPlayerBuffs.PositiveBuff = selectedBuff;

        while (!selected)
        {
            int rng = Random.Range(0, ListOfBuffs.Length);
            selectedBuff = ListOfBuffs[rng];
            if (selectedBuff.positiveBuff == true)
            {
                selected = true;
                currentPlayerBuffs.PositiveBuff = selectedBuff;
            }
        }

        selected = false;

        while (!selected)
        {
            int rng = Random.Range(0, ListOfBuffs.Length);
            selectedBuff = ListOfBuffs[rng];
            if (selectedBuff.positiveBuff == false && (selectedBuff.typeOfBuff != currentPlayerBuffs.PositiveBuff.typeOfBuff))
            {
                selected = true;
                currentPlayerBuffs.NegativeBuff = selectedBuff;
            }
        }

        return currentPlayerBuffs;
    }


}

[System.Serializable]
public struct STR_CurrentPlayerBuffs
{
    public SCR_BuffType PositiveBuff;
    public SCR_BuffType NegativeBuff;
}