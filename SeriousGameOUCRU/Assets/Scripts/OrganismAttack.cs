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
    public bool instantKill = true;


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

    private void OnTriggerStay2D(Collider2D c)
    {
        if (canAttack && !attackTarget)
        {
            attackTarget = c.GetComponentInParent<HumanCell>();

            // if target is a human cell and not already targeted
            if (attackTarget && !attackTarget.targetedBy)
            {
                attackTarget.targetedBy = this;
                if (!instantKill) attackTarget.GetComponent<OrganismMovement>().SetCanMove(false);

                // Set new attackTarget to move towards it
                if (orgMovement) orgMovement.SetTarget(attackTarget);
            }else
            {
                attackTarget = null;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        // If touch target, start to attack it
        if (!instantKill && canAttack && attackTarget && c.gameObject == attackTarget.gameObject)
        {
            StartCoroutine(AttackOverTime());
        }else if (instantKill)
        {
            InstantAttack(c.transform.GetComponentInParent<HumanCell>()); 
        }
    }

    private IEnumerator AttackOverTime()
    {
        canAttack = false;

        // Compute dot to apply and round it up
        int damageOverTime = attackTime == 0f ? attackTarget.GetHealth() : (int)((float)attackTarget.GetHealth() / attackTime) + 1;

        // While target alive we apply damage to it
        while (attackTarget)
        {
            targetLastPosition = attackTarget.transform.position;
            attackTarget.DamageOrganism(damageOverTime);

            if (!attackTarget) selfOrganism.InstantiateOrganism(targetLastPosition);
            yield return new WaitForSeconds(attackTarget ? 1f : 0f);
        }

        // Start recall to prevent chain attack
        StartCoroutine(AttackRecall());
    }

    private void InstantAttack(HumanCell target)
    {
        if (target)
        {
            target.DamageOrganism(target.GetHealth());

            if (target == attackTarget) 
            {
                // Reset target
                attackTarget = null;
                if (orgMovement) orgMovement.SetTarget(null);

                // Start recall to prevent chain attack
                StartCoroutine(AttackRecall());
            }
        }
    }

    public void UpdateDetectionColliderRadius(float bodyRadius)
    {
        detectionColl.radius = bodyRadius + attackRadius;
    }

    public void ResetTarget()
    {
        if (attackTarget)
        {
            attackTarget.targetedBy = null;
            attackTarget.GetComponent<OrganismMovement>().SetCanMove(true);
            attackTarget = null;
            orgMovement.SetTarget(null);
        }
    }
}
