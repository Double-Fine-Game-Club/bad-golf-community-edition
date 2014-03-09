using UnityEngine;
using System.Collections;

public class IntegrateHead : MonoBehaviour {
	
	// for interactive testing...
	void Update () {
		if(Input.GetKeyDown(KeyCode.H)) {
			GameObject body = GameObject.Find("RigtestBody"); // already in scene
			AddHead(body, "test/RigtestHead");
		}
	}

	
	public void AddHead(GameObject body, string headObjectID, string rigRootID="Rig")
	{
		GameObject head = (GameObject)Instantiate(Resources.Load(headObjectID)); // from prefab resource, not in scene yet
		if(head == null) {
			print("[AddHead] warning: Could not find object "+headObjectID);
			return;
		}

		// find rig roots in head and body
		Transform rigRootHead = null;
		foreach(Transform transform in head.transform.GetComponentsInChildren<Transform>()) {
			if(transform.name == rigRootID) {
				rigRootHead = transform;
			}
		}
		if (rigRootHead == null) {
			print ("[AddHead] Error: Rig root transform in head not found!");
			return;
		}

		Transform rigRootBody = null;
		foreach(Transform transform in body.transform.GetComponentsInChildren<Transform>()) {
			if(transform.name == rigRootID) {
				rigRootBody = transform;
			}
		}
		if (rigRootBody == null) {
			print ("[AddHead] Error: Rig root transform in body not found!");
			return;
		}

		Transform[] bonesInBody = rigRootBody.GetComponentsInChildren<Transform>();

		// reparent bones
		foreach (Transform boneFromHead in rigRootHead.GetComponentsInChildren<Transform>())
		{
			Transform boneFromParent = FindChildTransform(body.transform, bonesInBody, boneFromHead.name);
			if(boneFromParent != null) {
				print ("[AddHead] Reparenting bone "+boneFromHead.name);
				boneFromHead.transform.parent = boneFromParent.transform;
				boneFromHead.name = boneFromHead.name + "_fromHead";
			}
		}

		// attach head object
		head.transform.parent = body.transform;
	}

	private Transform FindChildTransform(Transform parent, Transform[] bonesInBody, string name)
	{
		foreach (Transform child in bonesInBody) {
			if(child.name == name) {
				return child;
			}
		}

		return null;
	}
}