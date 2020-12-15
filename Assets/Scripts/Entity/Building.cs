using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Entity, IObstacle
{
    [Header("Building")]
    [Tooltip("The type of resource the building accepts as a drop off.")]
    [SerializeField]
    private ResourceType resourceType;
    public ResourceType ResourceType => resourceType;

    [SerializeField]
    private bool isStartingBuilding;

    [Tooltip("The time in seconds it takes a single villager to contruct this building.")]
    [SerializeField]
    private float buildTime = 10;
    public float BuildTime => buildTime;

    private float remainingBuildTime;

    [Tooltip("The resources it cost to build this building.")]
    [SerializeField]
    private ResourceCost[] buildCosts = { new ResourceCost(ResourceType.wood, 100)}; // default cost of 100 Wood.
    public ResourceCost[] BuildCosts => buildCosts;

    private bool isBuilt;
    public bool IsBuilt => isBuilt;

    [Tooltip("The offset from the building where created units will spawn.")]
    [SerializeField]
    private Vector3 spawnOffset = new Vector3(0, 0, -1);

    private Vector3 SpawnPoint => (transform.position + spawnOffset);

    [SerializeField]
    private BuildingTask[] tasks;

    private Collider col;
    public Collider ObstacleCollider => col;

    private Queue<BuildingTask> activeTasks;

    private bool isWorking;

    private void Awake() {
        col = GetComponent<Collider>();
        isBuilt = isStartingBuilding;
        remainingBuildTime = buildTime;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        activeTasks = new Queue<BuildingTask>();
        isWorking = false;
    }

    private void OnDrawGizmos() {
        if (GameController.Instance != null && GameController.Instance.Debugging) {
            Gizmos.DrawSphere(SpawnPoint, 1);
        }
    }

    /// <summary>
    /// Starts the contruction of the building.
    /// </summary>
    public void StartBuilding() {
        // Check if there's enough resources
        if (!GameController.Instance.CheckResourceCost(buildCosts)) {
            Debug.LogWarning("Start Building Error: Cannot start building: " + this.name + "as there are not enough resources in store.");
            return;
        }

        // Remove building cost
        foreach(ResourceCost cost in buildCosts) {
            GameController.Instance.RemoveResourceCount(cost.ResourceType, cost.Cost);
        }

        enabled = false;
        isBuilt = false;
    }

    /// <summary>
    /// Decrements the build time by one second.
    /// </summary>
    public void UpdateBuildTime() {
        remainingBuildTime -= 1.0f;

        if(remainingBuildTime <= 0) {
            enabled = true;
            isBuilt = true;
            Debug.Log("Construction of: " + name + "complete");
        }
    }

    /// <summary>
    /// Queue the buildings task from it's task list at the index "task."
    /// </summary>
    /// <param name="task">The task to start</param>
    public void QueueTask(int task) {
        // Check if the building is enabled/built
        if (!IsBuilt)
            return;

        if (task > tasks.Length - 1) {
            Debug.LogWarning("Building Start Task Error: Provided index out of bounds");
            return;
        }

        if (tasks[task].Prerequisites()) {
            tasks[task].StartTask();
            activeTasks.Enqueue(tasks[task]);

            // Start work on the queue if we are not already.
            if (!isWorking)
                StartCoroutine(TaskQueue());
        }
        else {
            Debug.Log("Building Task Error: Trying to start task: " + tasks[task].name + ", but the prerequisites are not met.");
        }
    }

    /// <summary>
    /// Starts work on the queued tasks. Completes them in order of the queue and takes the sum of the tasks time to complete.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TaskQueue() {
        isWorking = true;

        // While there are tasks in the queue, complete them in order.
        while(activeTasks.Count > 0) {
            yield return StartCoroutine(DoTask());
        }

        isWorking = false;

        yield break;
    }

    /// <summary>
    /// Do the task that is in the front of the queue.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoTask() {
        // Find the current task
        BuildingTask task = activeTasks.Peek();

        // Wait for the task to complete
        yield return new WaitForSeconds(task.CompleteTime);

        // Complete the task
        task.Complete(SpawnPoint);

        // Remove the task from the queue
        activeTasks.Dequeue();

        // Done with task
        yield break;
    }

    protected override void Die() {
        base.Die();

        isWorking = false;

        // Stop working on queue
        StopAllCoroutines();

        Destroy(gameObject);
    }
}
