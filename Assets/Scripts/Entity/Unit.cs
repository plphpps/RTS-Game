using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Unit : Entity
{
    [Header("Unit")]
    [SerializeField]
    private int damage;

    [SerializeField]
    private float moveSpeed = 3.5f;

    [Tooltip("The minimum range this unit must be in in order to attack.")]
    [SerializeField]
    private float attackRange = 1f;

    [Tooltip("The time in seconds between attacks.")]
    [SerializeField]
    private float attackSpeed = 1f;

    // TODO: Use attacking state for debugging
    //private enum UnitState { idle, moving, attacking, gathering }

    private NavMeshAgent agent;
    public NavMeshAgent Agent => agent;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        if (GameController.Instance.Debugging)
            gameObject.GetComponent<Renderer>().material.color = Color.yellow;
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        // DEBUGGIN: Show state of unit.
        if (GameController.Instance.Debugging && agent.velocity == Vector3.zero)
            gameObject.GetComponent<Renderer>().material.color = Color.yellow;
        else if (GameController.Instance.Debugging)
            gameObject.GetComponent<Renderer>().material.color = Color.cyan;
    }

    public void Attack(Entity target) {
        if(target == null) {
            Debug.LogWarning("Unit Attack Error: " + name + " cannot attack as target provided is null.");
            return;
        }

        if(target.Team == Team) {
            Debug.LogWarning("Unit Attack Error: " + name + " cannot attack " + target.name + ", because their own the same team.");
            return;
        }

        // Start Attacking
        StopAllCoroutines();
        StartCoroutine(AttackRoutine(target));
    }

    protected virtual IEnumerator AttackRoutine(Entity target) {
        Collider targetCol = target.GetComponent<Collider>();
        Vector3 targetPos;

        // Find the target's position
        if (targetCol != null)
            targetPos = targetCol.ClosestPoint(transform.position);
        else
            targetPos = target.transform.position;

        // Wait for the Unit to move within range of the target's location.
        yield return StartCoroutine(AttackMoveToPoint(targetPos));

        // Attack while we're in range or until the target is dead
        while(Vector3.Distance(transform.position, targetPos) <= attackRange && target != null && !target.IsDead) {
            target.TakeDamage(damage);
            yield return new WaitForSeconds(attackSpeed);
        }

        // Check why we stopped attacking
        // The target is gone/dead
        if (target == null || target.IsDead)
            yield break;
        // The target has moved out of range
        else if (Vector3.Distance(transform.position, targetPos) > attackRange)
            Attack(target); // Attack the target again if they've moved out of range.

        // Done
        yield break;
    }

    /// <summary>
    /// Coroutine that moves the Unit to the specified point.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    protected IEnumerator MoveToPoint(Vector3 point) {
        Agent.SetDestination(point);
        yield return new WaitForSeconds(0.1f);
        while (Agent.velocity != Vector3.zero) {
            yield return null; // wait
        }
    }

    /// <summary>
    /// Coroutine that moves the Unit to the specified point or until they're in attack range.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    protected IEnumerator AttackMoveToPoint(Vector3 point) {
        Agent.SetDestination(point);
        yield return new WaitForSeconds(0.1f);
        while (Agent.velocity != Vector3.zero && Vector3.Distance(transform.position, point) > attackRange) {
            yield return null; // wait
        }
    }

    protected override void Die() {
        base.Die();

        // Stop the Units actions
        StopAllCoroutines();

        Destroy(gameObject);
    }
}
