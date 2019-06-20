using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismAttack : MonoBehaviour, IPooledObject
{
    /*** PUBLIC VARIABLES ***/

    [Header("Componenents")]
    public Rigidbody2D rb;

    [Header("Attack")]
    public float attackTime = 10f;
    public float attackRecallTime = 5f;
    public bool spawnCopyOnTargetDeath = false;


    /*** PRIVATE VARIABLES ***/

    // Self
    private Organism selfOrganism;
    private OrganismMovement orgMovement;

    // Attack
    private bool canAttack = false;
    private HumanCell attackTarget;
    
    // Cached
    private Vector2 targetLastPosition;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    private void Start()
    {
        orgMovement = GetComponent<OrganismMovement>();
    }


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        canAttack = false;
        attackTarget = null;

        targetLastPosition = Vector2.zero;

        // Cannot attack at spawn
        StartCoroutine(AttackRecall());
    }


    /***** ATTACK FUNCTIONS *****/

    private IEnumerator AttackRecall()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackRecallTime);
        canAttack = true;
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (canAttack)
        {
            attackTarget = c.GetComponentInParent<HumanCell>();

            // if target is a human cell and not already targeted
            if (attackTarget && !attackTarget.isTargeted)
            {
                attackTarget.isTargeted = true;

                // Set new attackTarget to move towards it
                orgMovement.SetTarget(attackTarget);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        // If touch target, start to attack it
        if (canAttack && c.gameObject == attackTarget.gameObject)
        {
            StartCoroutine(StartAttack());
        }
    }

    private IEnumerator StartAttack()
    {
        canAttack = false;

        // Compute dot to apply and round it up
        int damageOverTime = attackTime == 0f ? attackTarget.GetHealth() : (int)((float)attackTarget.GetHealth() / attackTime) + 1;

        // While target alive we apply damage to it
        while (attackTarget)
        {
            targetLastPosition = attackTarget.transform.position;
            attackTarget.DamageOrganism(damageOverTime);
            yield return new WaitForSeconds(1f);
        }

        // When target dead we spawn a new entity if requested
        if (spawnCopyOnTargetDeath)
        {
            selfOrganism.InstantiateOrganism(targetLastPosition);
        }

        // Start recall to prevent chain attack
        StartCoroutine(AttackRecall());
    }
}
