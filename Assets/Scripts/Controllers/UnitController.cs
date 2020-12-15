using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitController : MonoBehaviour
{
    [SerializeField]
    private int playerTeam;

    enum UnitFormation { Box, Circle }
    [Tooltip("The formation the units will arrive at their destination in.")]
    [SerializeField]
    private UnitFormation unitFormation;

    [SerializeField]
    private Image boxSelection;

    [Tooltip("How close the units will be once reaching their destination.")]
    [SerializeField]
    private float unitTightnessRadius;

    [Tooltip("List of buildings that can be placed by the player.")]
    [SerializeField]
    private GameObject[] buildingPrefabs;

    private bool isBuilding;

    // Mouse Positions
    private Vector3 startMousePos;          // The starting mouse point in Screen Space
    private Vector3 startMouseWorldPos;     // The starting mouse point in World Space
    private Vector3 currentMousePos;        // The current mouse point in Screen Space
    private Vector3 currentMouseWorldPos;   // The current mouse point in World Space

    // Current Selection
    private List<Unit> selectedUnits;
    private Building selectedBuilding;

    // Start is called before the first frame update
    void Start()
    {
        startMousePos = Input.mousePosition;
        startMouseWorldPos = Crosshair.MouseToWorldPos();
        boxSelection.gameObject.SetActive(false);
        isBuilding = false;

        selectedUnits = new List<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        // If we're building don't accept user input from here.
        if (isBuilding)
            return;
        
        // LMB has been pressed
        if (Input.GetMouseButtonDown(0)) {
            startMousePos = Input.mousePosition;
            startMouseWorldPos = Crosshair.MouseToWorldPos();
            boxSelection.gameObject.SetActive(true);

            SingleSelect();
        }

        // LMB is being held
        if (Input.GetMouseButton(0)) {
            currentMousePos = Input.mousePosition;
            currentMouseWorldPos = Crosshair.MouseToWorldPos();

            UpdateBoxSelection();
        }

        // LMB has been released
        if (Input.GetMouseButtonUp(0)) {
            BoxSelectUnits();
            boxSelection.gameObject.SetActive(false);
        }

        // RMB has been pressed
        if (Input.GetMouseButtonDown(1)) {
            CommandUnits(); // Move, Attack, Gather
        }

        // TODO: Add Villager Commands (Build, etc.)
        // Create House
        if (Input.GetKeyDown(KeyCode.Q)) {
            // Check if we have enough resources to build
            if (!GameController.Instance.CheckResourceCost(buildingPrefabs[0].GetComponent<Building>().BuildCosts)) {
                Debug.Log("Not enough resources to build: " + buildingPrefabs[0].name);
                return;
            }

            // Check if we have villagers selected to build.
            if (IsVillagerSelected())
                StartCoroutine(PlaceBuilding(buildingPrefabs[0]));
            else
                Debug.Log("Cannot build: " + buildingPrefabs[0].name + " as there are no villagers selected.");
        }

        // Create Barracks
        if (Input.GetKeyDown(KeyCode.W)) {
            // Check if we have enough resources to build
            if (!GameController.Instance.CheckResourceCost(buildingPrefabs[1].GetComponent<Building>().BuildCosts)) {
                Debug.Log("Not enough resources to build: " + buildingPrefabs[1].name);
                return;
            }

            // Check if we have villagers selected to build.
            if (IsVillagerSelected())
                StartCoroutine(PlaceBuilding(buildingPrefabs[1]));
            else
                Debug.Log("Cannot build: " + buildingPrefabs[1].name + " as there are no villagers selected.");
        }
    }

    private void OnDrawGizmos() {
        // DEBUGGING
        Gizmos.color = Color.green;
        if (Input.GetMouseButton(0)) {
            Vector3 scale = currentMouseWorldPos - startMouseWorldPos;
            Gizmos.DrawCube((currentMouseWorldPos + startMouseWorldPos) / 2, new Vector3(Mathf.Abs(scale.x), 10, Mathf.Abs(scale.z)));
        }
    }

    #region Commanding Units
    /// <summary>
    /// Update the box selection UI.
    /// </summary>
    private void UpdateBoxSelection() {
        float width = currentMousePos.x - startMousePos.x;
        float height = currentMousePos.y - startMousePos.y;

        boxSelection.rectTransform.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height)); // Set the width and height of the image. Use Abs value, as the width and height an be negative.
        boxSelection.rectTransform.anchoredPosition = startMousePos + new Vector3(width / 2, height / 2); // Set the anchored position to the bottom left of the image. Since the pivot point is in the middle, add half the width and height.
    }

    /// <summary>
    /// Set the currently box selected Units to the selected units.
    /// </summary>
    private void BoxSelectUnits() {
        Vector3 center = (currentMouseWorldPos + startMouseWorldPos) / 2;
        Vector3 scale = currentMouseWorldPos - startMouseWorldPos;
        scale.x = Mathf.Abs(scale.x);
        scale.y = Mathf.Abs(scale.y + 10); // Add a constant height to the y's scale.
        scale.z = Mathf.Abs(scale.z);

        Collider[] cols = Physics.OverlapBox(center, scale / 2);

        List<Unit> newSelectedUnits = new List<Unit>();
        foreach(Collider col in cols) {
            Unit u = col.GetComponent<Unit>();
            if (u != null && u.Team == playerTeam) {
                newSelectedUnits.Add(u);
            }
        }

        selectedUnits = newSelectedUnits;
    }

    /// <summary>
    /// Selects a single unit or building at the mouse position.
    /// </summary>
    private void SingleSelect() {
        // Determine the target
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
        Collider targetCol = hit.collider;

        // If nothing was hit do nothing
        if (targetCol == null)
            return;

        // Clear the selected units/building regardless of what is selected
        selectedUnits = new List<Unit>();
        selectedBuilding = null;

        // Selecting a Unit
        if (targetCol.GetComponent<Unit>()) {
            Unit u = targetCol.GetComponent<Unit>();
            if (playerTeam != u.Team) return; // Don't do anything if the unit isn't on our team

            selectedUnits.Add(u);
        }
        // Selecting a building
        else if (targetCol.GetComponentInParent<Building>()) { // Use get component in parent in case the building is made of multiple objects
            Building build = targetCol.GetComponentInParent<Building>();
            if (playerTeam != build.Team) return; // Don't do anything if the building isn't on our team

            selectedBuilding = build;

            // TODO: Move queueing tasks to game controller UI  / hotkey action
            selectedBuilding.QueueTask(0);
        }
    }

    /// <summary>
    /// Commands the selected units to act based on the target.
    /// </summary>
    private void CommandUnits() {
        // Determine target
        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
        Collider targetCol = hit.collider;

        // If nothing was hit do nothing
        if (targetCol == null)
            return;

        // Stop units current task if they recieve a new command.
        selectedUnits.ForEach(u => {
            u.StopAllCoroutines();
        });

        // DEBUGGING
        if(GameController.Instance.Debugging)
            print(targetCol.name);

        // Attack
        if (targetCol.GetComponent<Entity>()) {
            Entity target = targetCol.GetComponent<Entity>();

            Attack(target);         
        }
        // Gather
        else if (targetCol.GetComponent<ResourceNode>()){
            ResourceNode node = targetCol.GetComponent<ResourceNode>();

            Gather(node);
        }
        // Move
        else {
            Move(hit.point, unitFormation);
        }
    }

    /// <summary>
    /// Commands the selected units to attack the provided target.
    /// </summary>
    /// <param name="target">The target to attack.</param>
    private void Attack(Entity target) {
        // Don't do anything if the unit is on our team
        if (target.Team == playerTeam)
            return;

        selectedUnits.ForEach(u => {
            u.Attack(target);
        });
    }

    /// <summary>
    /// Commands the villagers in the current selection to gather from the targeted resource node.
    /// </summary>
    /// <param name="node">The node to be gathered from.</param>
    private void Gather(ResourceNode node) {
        selectedUnits.ForEach(u => {
            Villager vil = u.GetComponent<Villager>();

            if (vil != null)
                vil.Collect(node);
        });
    }

    /// <summary>
    /// Commands the selected units to move to a given point in a formation. When multiple units are selected they will move to a location around the provided point to avoid collisions.
    /// </summary>
    /// <param name="point">The point to move to.</param>
    /// <param name="formation">The formation the units will move in.</param>
    private void Move(Vector3 point, UnitFormation formation) {
        // Move all selected units in desired formation.
        // Box Formation
        if (unitFormation == UnitFormation.Box) {
            for(int i = 0; i < selectedUnits.Count; i++) {
                int row = (i < (selectedUnits.Count) / 2)? 0 : 1;
                Vector3 offset;
                if (row == 0)
                    offset = new Vector3(i * 2, 0, row);
                else
                    offset = new Vector3((i - selectedUnits.Count / 2) * 2, 0, row * 2);

                selectedUnits[i].Agent.SetDestination(point + offset);
            }
        }
        // Circle Formation
        else if (unitFormation == UnitFormation.Circle) {
            selectedUnits.ForEach(u => {
                // Get a random position with a circle based on the number of units selected and the desired tightness.
                Vector2 randCircle = Random.insideUnitCircle * unitTightnessRadius * (0.5f * selectedUnits.Count);
                Vector3 offset = new Vector3(randCircle.x, 0, randCircle.y);

                u.Agent.SetDestination(point + offset);
            });
        }
    }
    #endregion

    #region Build Command
    private IEnumerator PlaceBuilding(GameObject buildingPrefab) {
        isBuilding = true;

        // Create the building
        Vector3 offset = new Vector3(0, buildingPrefab.transform.position.y, 0);
        GameObject building = Instantiate(buildingPrefab, Crosshair.MouseToWorldPos() + offset, buildingPrefab.transform.rotation);
        Collider col = building.GetComponent<Collider>();
        Building b = building.GetComponent<Building>();
        b.enabled = false;

        // Make sure a building component is attached
        if(b == null) {
            Debug.LogWarning("Place Building Error: Provided building gameObject does not have Building component attached.");
            yield break;
        }

        // While we're still building, update the building to be at the mouse position and check for collisions
        while (isBuilding) {
            building.transform.position = Crosshair.MouseToWorldPos() + offset;

            // Try to place building if LMB is pressed
            if (Input.GetMouseButtonDown(0)) {
                // Check if the building's collider is overlapping with anything other than the ground and itself before placing
                Collider[] cols = Physics.OverlapBox(building.transform.position, col.bounds.extents);
                if (cols.Length > 2) {
                    Debug.Log("Not enough space to place: " + building.name + "here.");
                    yield return new WaitForFixedUpdate(); // Wait for end of fixed update to prevent freeze
                    continue;
                }

                // If there was space, command the selected villagers to build the building.
                selectedUnits.ForEach(u => {
                    Villager vil = u.GetComponent<Villager>();

                    if (vil != null)
                        vil.Build(b);
                });

                b.StartBuilding();

                isBuilding = false;
                yield break;
            }

            // Cancel build if RMB is pressed
            if (Input.GetMouseButtonDown(1)) {
                Debug.Log("RMB pressed.");
                Destroy(building);
                // wait a moment before setting is building to false, so that selected units don't try and follow commands
                yield return new WaitForSeconds(0.1f);
                isBuilding = false;
                yield break;
            }

            // Wait for end of fixed update to prevent freeze
            yield return new WaitForFixedUpdate();
        }

        // Done
        yield break;
    }
    #endregion

    /// <summary>
    /// Checks if there are villagers in the currently selected units.
    /// </summary>
    /// <returns>Returns true if there are villagers in the currently selected units. Returns false if there are no villagers selected.</returns>
    private bool IsVillagerSelected() {
        foreach(Unit u in selectedUnits) {
            if (u.GetComponent<Villager>())
                return true;
        }

        return false;
    }
}
