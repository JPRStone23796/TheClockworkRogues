using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_EnemyHitPoint : MonoBehaviour,IDamageable
{
    [SerializeField]  GameObject parentEnemyBody;
    private IEnemy parentEnemy;

    [SerializeField] private float damageMultiplier = 1.0f;

    void Awake()
    {
        parentEnemy = (IEnemy)parentEnemyBody.GetComponent(typeof(IEnemy));
     
    }

    public void DamageEnemy(float bulletDamage)
    {
        parentEnemy.UpdateHealth(bulletDamage * damageMultiplier);
    }
}
