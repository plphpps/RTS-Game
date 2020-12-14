using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField]
    private ResourceType resourceType;
    public ResourceType ResourceType => resourceType;

    [Tooltip("How much of \"resourceType\" resource this node starts with.")]
    [SerializeField]
    private int maxStore;

    private int currentStore;
    private bool isDepleated;
    public bool IsDepledted => isDepleated;

    // Start is called before the first frame update
    void Start()
    {
        currentStore = maxStore;
        isDepleated = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Decrements from the node's current store.
    /// </summary>
    public void CollectNode() {
        currentStore--;

        if(currentStore <= 0) {
            // TODO: Perhaps add an animation to a resource node being depleted.
            isDepleated = true;
            Destroy(gameObject, 1);
        }
    }
}
