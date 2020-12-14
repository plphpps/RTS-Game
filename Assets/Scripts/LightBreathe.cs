using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightBreathe : MonoBehaviour
{
    [SerializeField]
    private float maxIntensity = 1f;

    [SerializeField]
    private float minIntensity = 0.5f;

    [SerializeField]
    private float speed = 1f;

    private Light light;

    private float currentIntensity;

    private float min;
    private float max;
    private float time;

    // Start is called before the first frame update
    void Start() {
        light = GetComponent<Light>();

        currentIntensity = minIntensity;
        min = minIntensity;
        max = maxIntensity;
        time = 0.0f;
    }

    // Update is called once per frame
    void Update() {
        // Lerp from min to max intensity and set light intensity.
        currentIntensity = Mathf.Lerp(min, max, time);
        light.intensity = currentIntensity;
        time += Time.deltaTime * speed;

        // Once we've reached the max lerp back to the min (and vice versa).
        if(time > 1.0f) {
            float tmp = max;
            max = min;
            min = tmp;
            time = 0f;
        }
    }
}
