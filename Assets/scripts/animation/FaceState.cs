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
		if (Input.GetKeyDown (KeyCode.W)) {
			happyTargetVal = 100;
		} else if (Input.GetKeyUp (KeyCode.W)) {
			happyTargetVal = 0;
		}

		if (Input.GetKeyDown (KeyCode.S)) {
			angryTargetVal = 100;
		} else if (Input.GetKeyUp (KeyCode.S)) {
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

	void SetFaceExpression(string expression)
	{
		switch (expression) {
		case "Happy":
			if (happyIndex > -1) happyTargetVal = 100;
			if (angryIndex > -1) angryTargetVal = 0;
			break;
		case "Angry":
			if (happyIndex > -1) happyTargetVal = 0;
			if (angryIndex > -1) angryTargetVal = 100;
			break;
		case "Neutral":
			if (happyIndex > -1) happyTargetVal = 0;
			if (angryIndex > -1) angryTargetVal = 0;
			break;
		default:
			break;
		}
		return;
	}
}
