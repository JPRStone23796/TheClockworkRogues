using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




[System.Serializable]
public abstract class SCR_Ability : ScriptableObject
{
    public string abilityName = String.Empty;
    public Image abilityIcon;
    GameObject playerParent;
    public string description;
    public abstract void CarryOutAbility();
    public abstract void SetPlayerParent(GameObject mPlayer);


}
