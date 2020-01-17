using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_LevelMaterials : MonoBehaviour
{

    [SerializeField] private Material nonEmissiveMaterial;

    public void setObjectNonEmissive()
    {
        GetComponent<Renderer>().material = nonEmissiveMaterial;
    }

}
