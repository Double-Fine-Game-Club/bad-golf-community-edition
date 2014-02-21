using UnityEngine;
using System.Collections;

// Testing control of face blend shapes. TODO: Add functions to switch faces that can be called from other scripts
public class FaceState : MonoBehaviour {

	SkinnedMeshRenderer meshRenderer;
	Mesh mesh;

	// blend shape indices
	int happyIndex = -1;
	int angryIndex = -1;

	float happyCurrentVal = 0;
	float happyTargetVal = 0;

	float angryCurrentVal = 0;
	float angryTargetVal = 0;

	// Use this for initialization
	void Start () {
		meshRenderer = GetComponent<SkinnedMeshRenderer>();

		if (meshRenderer) {
			mesh = meshRenderer.sharedMesh;

			happyIndex = mesh.GetBlendShapeIndex("Happy");
			angryIndex = mesh.GetBlendShapeIndex("Angry");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!meshRenderer) return;

		// Manual face change for testing (or for the lulz?)
		if (Input.GetKeyDown (KeyCode.F)) {
			happyTargetVal = 100;
		} else if (Input.GetKeyUp (KeyCode.F)) {
			happyTargetVal = 0;
		}

		if (Input.GetKeyDown (KeyCode.G)) {
			angryTargetVal = 100;
		} else if (Input.GetKeyUp (KeyCode.G)) {
			angryTargetVal = 0;
		}

		// temp, TODO refine!
		happyCurrentVal = (float) (0.5 * happyCurrentVal + 0.5 * happyTargetVal);
		angryCurrentVal = (float) (0.5 * angryCurrentVal + 0.5 * angryTargetVal);

		if (happyIndex > -1)
			meshRenderer.SetBlendShapeWeight (happyIndex, happyCurrentVal);
		if(angryIndex > -1)
			meshRenderer.SetBlendShapeWeight (angryIndex, angryCurrentVal);
	}
}
