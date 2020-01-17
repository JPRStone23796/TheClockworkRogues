using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_GunRotation : MonoBehaviour
{

    [SerializeField]
    float RotationSpeed = 1.0f;


    private void Update()
    {
        Vector3 rot = new Vector3(0, RotationSpeed * Time.deltaTime, 0);
        transform.Rotate(rot);

    }


    

}
