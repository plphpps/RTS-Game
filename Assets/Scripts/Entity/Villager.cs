using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Villager : Unit
{
    [SerializeField]
    private int carryingCapacity = 10; // The maximum amount a villager can carry.

    [Tooltip("The rate in seconds that the villager collects 1 resource from a resource node.")]
    [SerializeField]
    private float collectionRate = 1;

    [Tooltip("The radius in which a villager will look for a new resource node matching their current resource. Villagers look for new nodes after their current node has been depleted.")]
    [SerializeField]
    private float collectionRadius = 20f;

    private int carryingAmount; // The current amount of a given resource the villager is carrying.

    private ResourceType resourceType; // The resource type the villager is carrying.

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();

        carryingAmount = 0;
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
    }

    /// <summary>
    /// Commands the villager to collect from a given resource node until it and surrounding nodes are depleted.
    /// </summary>
    /// <param name="node"></param>
    public void Collect(ResourceNode node) {
        // If the node is null, don't do anything.
        if (node == null)
            return;

        // Reset villagers carrying amount and resource type if a new resource has been selected.
        if (resourceType != node.ResourceType) {
            resourceType = node.ResourceType;
            carryingAmount = 0;
        }

        // Start Collecting
        StopAllCoroutines();
        StartCoroutine(CollectNode(node));
    }

    private IEnumerator CollectNode(ResourceNode node) {
        Vector3 nodePos = node.transform.position;

        // Move to selected node. Wait to collect until arrived.
        yield return StartCoroutine(MoveToPoint(nodePos));

        // Collect node until full or until node is depleted
        while (carryingAmount < carryingCapacity && node != null && !node.IsDepledted) {
            Debug.Log("Mining");
            // TODO: Perhaps play some kind of animation here.
            node.CollectNode();
            carryingAmount++;

            // Wait until ready to collect again
            yield return new WaitForSeconds(collectionRate);
        }

        // Drop off resources at closest drop off point if needed
        if (carryingAmount == carryingCapacity) {
            // Go to drop off point
            // Wait until we're finished dropping off
            yield return StartCoroutine(DropOff(SearchDropOff()));
        }

        // Return to node position. Find new node if current node is depleted.
        yield return StartCoroutine(MoveToPoint(nodePos));

        // Find new node is current node is gone/depleted
        if (node == null || node.IsDepledted) {
            ResourceNode newNode = SearchNode();

            // If no new node in collection radius, drop off resource, return, and quit working.
            if (newNode == null && carryingAmount > 0) {
                // Drop off
                yield return StartCoroutine(DropOff(SearchDropOff()));
                // Return
                yield return StartCoroutine(MoveToPoint(nodePos));
                // Quit
                yield break;
            }
            // Repeat process on new node.
            else
                Collect(newNode);
        }
        // Repeat process on current node
        else {
            Collect(node);
        }

        // Done working
        yield break;
    }

    /// <summary>
    /// Coroutine that moves the Villager to the specified point.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private IEnumerator MoveToPoint(Vector3 point) {
        Agent.SetDestination(point);
        yield return new WaitForSeconds(0.1f);
        while (Agent.velocity != Vector3.zero) {
            yield return null; // wait
        }
    }

    /// <summary>
    /// Drop off the Villager's current load, updating the amount of resources currently stored.
    /// </summary>
    private IEnumerator DropOff(Building dropPoint) {
        // If no drop off can be found, quit
        if(dropPoint == null) {
            Debug.LogWarning("Villager " + name + " could not find drop off point for " + resourceType + ".");
            yield break;
        }

        // Move to drop off point
        yield return StartCoroutine(MoveToPoint(dropPoint.transform.position));

        // Drop off
        Debug.Log("Villager " + name + " dropped off " + carryingAmount + " " + resourceType.ToString() + " to " + dropPoint.name + ".");
        GameController.Instance.AddResourceCount(resourceType, carryingAmount);
        carryingAmount = 0;

        // Done dropping off.
        yield break;
    }

    /// <summary>
    /// Search for the closest new resource node in the area that matches the Villager's current resource type.
    /// </summary>
    /// <returns></returns>
    private ResourceNode SearchNode() {
        // Find nodes within collection radius
        Collider[] cols = Physics.OverlapSphere(transform.position, collectionRadius);

        // Find the closest node in the radius that matches our resource type.
        float minDist = float.MaxValue;
        ResourceNode closest = null;
        foreach(Collider col in cols){
            ResourceNode node = col.GetComponent<ResourceNode>();
            if (node == null || resourceType != node.ResourceType) continue;

            float dist = Vector3.Distance(transform.position, node.transform.position);
            if(dist < minDist) {
                minDist = dist;
                closest = node;
            }
        }

        return closest;
    }

    /// <summary>
    /// Search for the closest drop off building that allows for the villager to drop off their current resource type.
    /// </summary>
    /// <returns></returns>
    private Building SearchDropOff() {
        // Find drop off building within collection radius
        Collider[] cols = Physics.OverlapSphere(transform.position, collectionRadius);

        // Find the closest drop off building in the radius that matches our resource type
        float minDist = float.MaxValue;
        Building closest = null;
        foreach (Collider col in cols) {
            Building building = col.GetComponent<Building>();
            // If the col isn't a building or the building doesn't allow us to drop off our resource, skip it.
            // Resource type is a Flag enum, meaning it can have multiple values. So we use a bitwise and to check if the building includes our resource type.
            if (building == null || Team != building.Team || ((resourceType & building.ResourceType) != resourceType)) continue;

            float dist = Vector3.Distance(transform.position, building.transform.position);
            if (dist < minDist) {
                minDist = dist;
                closest = building;
            }
        }

        // If no building was in close proximity, scan for any building we can drop off to.
        if(closest == null) {
            Building[] dropOffs = FindObjectsOfType<Building>();

            // Find the closest of these buildings that is on our team
            minDist = float.MaxValue;
            foreach (Building build in dropOffs) {
                // If the col isn't a building or the building doesn't allow us to drop off our resource, skip it.
                // Resource type is a Flag enum, meaning it can have multiple values. So we use a bitwise and to check if the building includes our resource type.
                if (Team != build.Team || (resourceType & build.ResourceType) != resourceType) continue;

                float dist = Vector3.Distance(transform.position, build.transform.position);
                if (dist < minDist) {
                    minDist = dist;
                    closest = build;
                }
            }
        }

        return closest;
    }
}
