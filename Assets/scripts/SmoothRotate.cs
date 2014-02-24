using UnityEngine;
using System.Collections;

public class SmoothRotate : MonoBehaviour {

    // Target to rotate around
    public Transform target;

    // Multiplier for mousemovement
    public float mouseMultiplier;

    void LateUpdate() {

        // Check if mouse has moved
        if (Input.GetAxis("Mouse X") != 0) {
            transform.RotateAround(target.position, Vector3.up, Input.GetAxis("Mouse X") * mouseMultiplier * Time.deltaTime);
        }
        if (Input.GetAxis("Mouse Y") != 0) {
            transform.RotateAround(target.position, Vector3.left, Input.GetAxis("Mouse Y") * mouseMultiplier * Time.deltaTime);
        }
    }
}
