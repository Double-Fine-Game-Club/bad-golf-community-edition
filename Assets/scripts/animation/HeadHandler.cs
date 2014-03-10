using UnityEngine;
using System.Collections;

public class HeadHandler : MonoBehaviour {

	/**
	 * Adds an instance of the named headObject to the body and integrates the rig.
	 * Returns the created head object
	 */
	public static GameObject AddHead(GameObject body, string headModel, string rootBone="Rig")
	{
		// TODO make sure body does not already have a head?

		GameObject head = (GameObject)Instantiate(Resources.Load(headModel)); // from prefab resource, not in scene yet
		if(head == null) {
			print("[AddHead] warning: Could not find object "+headModel);
			return null;
		}

		// find rig roots in head and body
		Transform rigRootHead = null;
		foreach(Transform transform in head.transform.GetComponentsInChildren<Transform>()) {
			if(transform.name == rootBone) {
				rigRootHead = transform;
			}
		}
		if (rigRootHead == null) {
			print ("[AddHead] Error: Rig root transform in head not found!");
			return null;
		}

		Transform rigRootBody = null;
		foreach(Transform transform in body.transform.GetComponentsInChildren<Transform>()) {
			if(transform.name == rootBone) {
				rigRootBody = transform;
			}
		}
		if (rigRootBody == null) {
			print ("[AddHead] Error: Rig root transform in body not found!");
			return null;
		}

		Transform[] bonesInBody = rigRootBody.GetComponentsInChildren<Transform>();

		// reparent bones
		foreach (Transform boneFromHead in rigRootHead.GetComponentsInChildren<Transform>())
		{
			Transform boneFromParent = FindChildTransform(body.transform, bonesInBody, boneFromHead.name);
			if(boneFromParent != null) {
				print ("[AddHead] Reparenting bone "+boneFromHead.name);

				boneFromHead.transform.parent = boneFromParent.transform;
				boneFromHead.transform.localPosition = Vector3.zero;
				boneFromHead.transform.localRotation = Quaternion.identity;

				boneFromHead.name = boneFromHead.name + "_fromHead";
			}
		}

		// attach head object and clear transform (has to fit from model!)
		head.transform.parent = body.transform;
		head.transform.localPosition = Vector3.zero;
		head.transform.localRotation = Quaternion.identity;

		// TODO save head info in networkVariables, propagate (where's the best place to do this?)
		return head;
	}

	/**
	 * Remove head GameObject and rig elements from body
	 * TODO Is this compatible with the location of the PlayerInfo in local/networked games?
	 */
	public static void RemoveHead (GameObject body, GameObject head)
	{
		if(head.transform.parent != body.transform) {
			print ("[RemoveHead] Warning: head is not child of body, aborting...");
			return;
		}

		// remove bones
		Transform[] bonesInBody = body.transform.GetComponentsInChildren<Transform>();
		foreach(Transform boneInBody in bonesInBody) {
			if(boneInBody.name.IndexOf("_fromHead") >= 0) {
				print ("[RemoveHead] Removing bone "+boneInBody.name);
				boneInBody.transform.parent = null;
				Destroy(boneInBody.gameObject);
			}
		}

		print ("[RemoveHead] Removing head object "+head.name);
		head.transform.parent = null;
		Destroy(head);
	}

	/**
	 * Convenience function
	 */
	public static void SwitchHead(GameObject body, GameObject previousHead, string headModel, string rootBone)
	{
		RemoveHead(body, previousHead);
		AddHead(body, headModel, rootBone);
	}

	// ---------------------------------------------------------
	private static Transform FindChildTransform(Transform parent, Transform[] bonesInBody, string name)
	{
		foreach (Transform child in bonesInBody) {
			if(child.name == name) {
				return child;
			}
		}

		return null;
	}
}