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

        selectedUnits = new List<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Allow for selection of singular units (i.e. clicking a single unit vs always having to box select).

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
    }

    private void OnDrawGizmos() {
        // DEBUGGING
        Gizmos.color = Color.green;
        if (Input.GetMouseButton(0)) {
            Vector3 scale = currentMouseWorldPos - startMouseWorldPos;
            Gizmos.DrawCube((currentMouseWorldPos + startMouseWorldPos) / 2, new Vector3(Mathf.Abs(scale.x), 10, Mathf.Abs(scale.z)));
        }
    }

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

        // Stop villagers from working if they recieve a new command.
        selectedUnits.ForEach(u => {
            Villager vil = u.GetComponent<Villager>();
            if (vil != null)
                vil.StopAllCoroutines();
        });

        // DEBUGGING
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

        // Attack the target
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
}
