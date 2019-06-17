using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacteriaCell : Cell, IPooledObject
{
    /*** PUBLIC VARIABLES ***/

    [Header("Conjugaison")]
    public float conjugaisonProba = 0.1f;
    public float conjugaisonRecallTime = 5f;

    [Header("Transformation")]
    public float transformationProbability = 0.5f;

    [Header("Attack")]
    public float rushForce = 10f;
    public float detectionRange = 7f;

    public static List<BacteriaCell> bacteriaCellList = new List<BacteriaCell>();

    /*** PRIVATE/PROTECTED VARIABLES ***/

    private RandomMovement rm;
    private GenericObjectPool<BacteriaCell> bacteriaCellPool;

    // Conjugaison
    private bool canCollide = false;

    // Shield
    private Shield shieldScript;

    // Cell attack
    private GameObject targetCell = null;

    private SphereCollider detectionCollider;


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    protected override void Awake()
    {
        base.Awake();

        // TEMP to get initial proba
        if (BacteriaCell.bacteriaCellList.Count == 1)
            GameController.Instance.globalMutationProba = mutationProba;

        // Init movement component
        rm = GetComponent<RandomMovement>();

        // Initialize shield script component
        shieldScript = GetComponent<Shield>();
    }

    protected override void Start()
    {
        base.Start();
        
        bacteriaCellPool = BacteriaCellPool.Instance;

        // To avoid conjugaison on spawn
        StartCoroutine(CollidingRecall());
    }

    protected override void InitComponents()
    {
        base.InitComponents();

        // Initialize components
        coll = transform.GetChild(0).GetComponent<SphereCollider>();
        detectionCollider = transform.GetChild(1).GetComponent<SphereCollider>();
    }

    public override void OnObjectToSpawn()
    {
        base.OnObjectToSpawn();

        bacteriaCellList.Add(this);
        cellSize = baseCellSize;
        shieldScript.ActivateShield();
        rm.SetCanMove(true);

        uiController.UpdateBacteriaCellCount(bacteriaCellList.Count);
    }

    protected override void Update()
    {
        base.Update();
        
        // Check is game is not currently paused
        if (!gameController.IsGamePaused())
        {
            // Attempt to mutate cell every frame
            TryToMutateCell();
        }
    }


    /***** SIZE FUNCTIONS *****/

    public void UpdateCellSize()
    {
        // Update size for spawning purposes
        cellSize = shieldScript.GetShieldSize().x;
        
        
        // Update colldier size
        detectionCollider.radius = cellSize / 2 + detectionRange;
    }


    /***** MUTATION FUNCTIONS *****/

    protected override void TryToMutateCell()
    {
        // If mutation is triggered
        if (Random.Range(0f, 1f) < mutationProba)
        {
            if (isResistant)
            {
                // If shield is already activated, duplicate it
                shieldScript.DuplicateShield();
            }else
            {
                // Activate shield for the first time
                ActivateResistance();
            }
            UpdateCellSize();
        }
    }

    public void IncreaseMutationProba(float increase)
    {
        mutationProba += increase;
    }


    /***** DUPLICATION FUNCTIONS *****/

    protected override GameObject InstantiateCell(Vector3 randomPos)
    {
        BacteriaCell bacteriaCellToSpawn = bacteriaCellPool.Get();

        bacteriaCellToSpawn.ResetOrganismAtPosition(randomPos);
        bacteriaCellToSpawn.OnObjectToSpawn();

        if (isResistant)
        {
            bacteriaCellToSpawn.GetShieldScript().SetShieldHealth(shieldScript.GetShieldHealth());
        }

        return bacteriaCellToSpawn.gameObject;
    }

    
    /***** CONJUGAISON FUNCTIONS *****/

    // Collision event called by both cell and shield on collision
    public void CollisionEvent(Collision collision)
    {
        if (canCollide)
        {
            // Start coroutine to prevent multiColliding
            StartCoroutine(CollidingRecall());
            
            // Try to trigger the conjugaison
            TryToConjugateCell(collision.gameObject.GetComponentInParent<Shield>());
        }
    }

    // Buffer to prevent collision for a short time
    public IEnumerator CollidingRecall()
    {
        canCollide = false;
        yield return new WaitForSeconds(conjugaisonRecallTime); // Time to wait before it can collide again
        canCollide = true;
    }

    // Process to trigger the conjugaison
    private void TryToConjugateCell(Shield s)
    {
        // If collided object is a shield and conjugaison chance is triggered
        if (s && Random.Range(0, 1) < conjugaisonProba)
        {
            // If first time we activate resistance
            if (!isResistant)
            {
                ActivateResistance();
            }

            // Change shield health if collided object has a larger health amount
            if (s.GetShieldHealth() > shieldScript.GetShieldHealth())
                shieldScript.SetShieldHealth(s.GetShieldHealth());
        }
    }

    public bool CanCollide()
    {
        return canCollide;
    }


    /***** HEALTH FUNCTIONS *****/

    // Apply damage to cell if shield health is at 0, otherwise damage the shield
    public override void DamageOrganism(int dmg)
    {
        if (shieldScript.GetShieldHealth() == 0)
        {
            base.DamageOrganism(dmg);
        }else
        {
            shieldScript.DamageShield(dmg);
        }
    }

    // Called when the cell has to die
    public override void KillOrganism()
    {
        // Prevent player to keep targeting bacteria
        playerController.ResetTarget();

        // Stop moving
        rm.SetCanMove(false);

        // Increase killed count
        gameController.IncrementBacteriaCellKillCount();

        // Remove from list
        RemoveFromList();

        base.KillOrganism();
    }

    protected override void DisolveOverTime()
    {
        base.DisolveOverTime();

        // Desactivate shield for disolve
        shieldScript.DesactivateShield();
    }

    protected override void DestroyOrganism()
    {
        // Put back this bacteria to the pool to be reused
        bacteriaCellPool.ReturnToPool(this);
    }


    /***** LIST FUNCTIONS *****/
    
    private void RemoveFromList()
    {
        bacteriaCellList.Remove(this);
        uiController.UpdateBacteriaCellCount(bacteriaCellList.Count);

        if (BacteriaCell.bacteriaCellList.Count == 0 && Virus.virusList.Count == 0)
        {
            gameController.PlayerWon();
        }
    }


    /***** TRIGGER FUNCTIONS *****/

    private void OnTriggerEnter(Collider c)
    {
        if (c.gameObject.GetComponent<HumanCell>() && !targetCell)
        {
            // Set cell as target when trigger detect one
            targetCell = c.gameObject;

            // Move toward cell to kill it
            StartCoroutine(MoveTowardTarget());
        }
    }

    private IEnumerator MoveTowardTarget()
    {
        // Stop random movement
        rm.SetCanMove(false);

        while (targetCell)
        {
            // Compute movement direction
            Vector3 moveDirection = targetCell.transform.position - transform.position;
            moveDirection.Normalize();

            // Move toward target while it's still alive
            rb.AddForce(moveDirection * rushForce, ForceMode.Impulse);
            
            // Wait for next frame to continue
            yield return new WaitForEndOfFrame();
        }

        // Start to move again if didn't target any other cell
        if (!targetCell)
            rm.SetCanMove(true);
    }

    public void UnTargetCell()
    {
        targetCell = null;
    }

    public Shield GetShieldScript()
    {
        return shieldScript;
    }
}
