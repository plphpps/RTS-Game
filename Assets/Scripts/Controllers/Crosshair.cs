using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
   
    }

    public static Vector3 MouseToWorldPos() {
        Vector3 worldPos = new Vector3();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero); // Simulate ground so that the cursor works even above holes in the ground
        if (ground.Raycast(ray, out float dist)) { // Draw a ray from the camera to the plane
            worldPos = ray.GetPoint(dist);
        }

        Debug.DrawLine(Camera.main.transform.position, worldPos, Color.red);
        Debug.DrawLine(worldPos, new Vector3(worldPos.x, 1000, worldPos.z), Color.green);

        return worldPos;
    }
}
