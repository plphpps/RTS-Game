using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Unit : Entity
{
    [SerializeField]
    private int damage;

    [SerializeField]
    private float moveSpeed = 3.5f;

    [Tooltip("The acceptable stop distance from a provided path. Ensures Units do not get stuck moving forever.")]
    [SerializeField]
    protected float pathThreshhold = 0.5f;

    private NavMeshAgent agent;
    public NavMeshAgent Agent => agent;

    private Vector3 goal;

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

    protected override void Die() {
        base.Die();

        // Stop the Units actions
        StopAllCoroutines();

        Destroy(gameObject);
    }
}
