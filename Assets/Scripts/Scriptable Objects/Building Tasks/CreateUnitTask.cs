using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreateUnitTask", menuName = "ScriptableObjects/BuildingTasks/CreateUnitTask")]
public class CreateUnitTask : BuildingTask {

    [Tooltip("The unit to be created.")]
    [SerializeField]
    private GameObject unitPrefab;

    private void OnValidate() {
        if (unitPrefab != null && !unitPrefab.GetComponent<Unit>())
            Debug.LogWarning("Create Unit Task Warning: provided unit gameObject: " + unitPrefab.name + " does not have a Unit component attached.");
    }

    protected override void Result(Vector3 resultPos) {
        // Spawn the unit at the given position.
        Instantiate(unitPrefab, resultPos, unitPrefab.transform.rotation);
    }
}
