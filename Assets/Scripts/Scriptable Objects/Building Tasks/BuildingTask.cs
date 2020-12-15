using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingTask : ScriptableObject
{
    [Tooltip("The time it takes for the task to complete")]
    [SerializeField]
    private float completeTime = 5f;
    public float CompleteTime => completeTime;

    [SerializeField]
    private ResourceCost[] costs;

    /// <summary>
    /// The results of starting a task.
    /// </summary>
    public virtual void StartTask() {
        if (!Prerequisites()) {
            //Debug.Log("Do not meet the prerequisite for the task: " + name);
            return;
        }

        // Remove the resource cost from what is stored.
        foreach (ResourceCost cost in costs) {
            GameController.Instance.RemoveResourceCount(cost.ResourceType, cost.Cost);
        }
    }

    /// <summary>
    /// The prerequisite for the task to be initiated/completed.
    /// </summary>
    /// <returns></returns>
    public virtual bool Prerequisites() {
        // Check if there is currently enough resources in store to cover the cost of the task.
        foreach(ResourceCost cost in costs) {
            int stored = GameController.Instance.GetResourceCount(cost.ResourceType);

            // There is not enough resource in store.
            if ((stored - cost.Cost) < 0)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Complete the task and cause the end result.
    /// </summary>
    /// <param name="resultPos"></param>
    public virtual void Complete(Vector3 resultPos) {
        Result(resultPos);
    }

    /// <summary>
    /// What occurs after the task has been completed.
    /// </summary>
    protected abstract void Result(Vector3 resultPos);
}
