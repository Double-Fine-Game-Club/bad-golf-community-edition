using UnityEngine;
using System.Collections;

public class networkLevelLoad : MonoBehaviour {
	public bool NetworkStart;
	networkVariables nvs;
	PlayerInfo myInfo;

	// Use this for initialization
	void Start () {
		if (NetworkStart==true) {
			Transform lvlPrev = GameObject.Find("LEVEL_PREVIEW").transform;
			lvlPrev.FindChild("fatmandu_course1_tweaks").gameObject.SetActive(false);
			(lvlPrev.FindChild("previewCamera").GetComponent(typeof(Orbit)) as Orbit).enabled = false;
			lvlPrev.FindChild("previewCamera").position = new Vector3(-36.73973f,26.18f,-82.74622f);
			lvlPrev.FindChild("previewCamera").Rotate(30f,180f,4f);
			gameObject.AddComponent (typeof(NetworkView));
			NetworkView netView = gameObject.GetComponent (typeof(NetworkView)) as NetworkView;
			netView.gameObject.AddComponent (typeof(networkManager));
			nvs = netView.gameObject.AddComponent (typeof(networkVariables)) as networkVariables;
		}
	}

	public void StartLoad(bool startLoad){
		NetworkStart = startLoad;
	}
}
