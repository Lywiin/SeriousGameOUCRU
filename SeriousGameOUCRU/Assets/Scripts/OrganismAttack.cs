using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrganismAttack : MonoBehaviour, IPooledObject
{
    /*** PUBLIC VARIABLES ***/

    [Header("Componenents")]
    public Rigidbody2D rb;
    public CircleCollider2D detectionColl;

    [Header("Attack")]
    public float attackRadius = 10f;
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
        selfOrganism = GetComponent<Organism>();
        orgMovement = GetComponent<OrganismMovement>();
    }


    /***** POOL FUNCTIONS *****/

    public virtual void OnObjectToSpawn()
    {
        detectionColl.radius = attackRadius;

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
                attackTarget.GetComponent<OrganismMovement>().SetCanMove(false);

                // Set new attackTarget to move towards it
                orgMovement.SetTarget(attackTarget);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        // If touch target, start to attack it
        if (canAttack && attackTarget && c.gameObject == attackTarget.gameObject)
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
        while (attackTarget.gameObject.activeSelf)
        {
            targetLastPosition = attackTarget.transform.position;
            attackTarget.DamageOrganism(damageOverTime);
            yield return new WaitForSeconds(1f);
        }

        // Reset target
        attackTarget = null;
        orgMovement.SetTarget(null);

        // When target dead we spawn a new entity if requested
        if (spawnCopyOnTargetDeath)
        {
            selfOrganism.InstantiateOrganism(targetLastPosition);
        }

        // Start recall to prevent chain attack
        StartCoroutine(AttackRecall());
    }

    public void UpdateDetectionColliderRadius(float bodyRadius)
    {
        detectionColl.radius = bodyRadius + attackRadius;
    }
}
