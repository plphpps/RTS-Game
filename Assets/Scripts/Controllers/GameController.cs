using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    // singleton
    private static GameController instance;
    public static GameController Instance => instance;

    [Header("Options")]
    [SerializeField]
    private bool isPaused;
    public bool IsPaused => isPaused;

    [SerializeField]
    private bool debugging;
    public bool Debugging { get { return debugging; } }

    [Header("UI")]
    [SerializeField]
    private Text foodCount;
    [SerializeField]
    private Text woodCount;
    [SerializeField]
    private Text goldCount;
    [SerializeField]
    private Text stoneCount;

    // Resources
    // Perhaps add some forward facing properties so these values can be accessed elsewhere.
    private int food;
    private int wood;
    private int gold;
    private int stone;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else {
            instance = this;
        }
    }

    private void Start() {
        // Reset Resource Counts
        food = 0;
        wood = 0;
        gold = 0;
        stone = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            isPaused = !isPaused;
        }
    }

    /// <summary>
    /// Adds to the current amount of the given resource stored.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    public void AddResourceCount(ResourceType type, int amount) {
        UpdateResourceCount(type, amount);
    }

    /// <summary>
    /// Removes the from the current amount of a gien resource stored.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    public void RemoveResourceCount(ResourceType type, int amount) {
        UpdateResourceCount(type, -amount);
    }

    /// <summary>
    /// Updates the current amount of the given resource stored.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    private void UpdateResourceCount(ResourceType type, int amount) {
        switch (type) {
            case ResourceType.food:
                food = Mathf.Clamp(food + amount, 0, int.MaxValue);
                foodCount.text = "Food: " + food.ToString();
                break;
            case ResourceType.wood:
                wood = Mathf.Clamp(wood + amount, 0, int.MaxValue);
                woodCount.text = "Wood: " + wood.ToString();
                break;
            case ResourceType.gold:
                gold = Mathf.Clamp(gold + amount, 0, int.MaxValue); ;
                goldCount.text = "Gold: " + gold.ToString();
                break;
            case ResourceType.stone:
                stone = Mathf.Clamp(stone + amount, 0, int.MaxValue); ;
                stoneCount.text = "Stone: " + stone.ToString();
                break;
            default:
                Debug.LogError("Update Resourse Count Error: Invalid resource type.");
                return;
        }
    }

    /// <summary>
    /// Returns how much of a given resource is currently stored.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int GetResourceCount(ResourceType type) {
        switch (type) {
            case ResourceType.food:
                return food;
            case ResourceType.wood:
                return wood;
            case ResourceType.gold:
                return gold;
            case ResourceType.stone:
                return stone;
            default:
                Debug.LogError("Get Resourse Count Error: Invalid resource type.");
                return -1;
        }
    }
}
