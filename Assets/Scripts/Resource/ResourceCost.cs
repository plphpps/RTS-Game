using UnityEngine;

[System.Serializable]
public class ResourceCost
{
    [SerializeField]
    private ResourceType resourceType;
    [SerializeField]
    private int cost;

    public ResourceType ResourceType => resourceType;
    public int Cost => cost;

    public ResourceCost(ResourceType resourceType, int cost) {
        this.resourceType = ResourceType;
        this.cost = cost;
    }
}
