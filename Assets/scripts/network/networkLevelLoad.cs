using UnityEngine;
using System.Collections;

public class networkLevelLoad : MonoBehaviour {
	public bool NetworkStart;
	// Use this for initialization
	void Start () {
		if (NetworkStart==true) {
			gameObject.AddComponent (typeof(NetworkView));
			NetworkView netView = gameObject.GetComponent (typeof(NetworkView)) as NetworkView;
			netView.gameObject.AddComponent (typeof(networkManager));
			netView.gameObject.AddComponent (typeof(networkVariables));
		}
	}

	public void StartLoad(bool startLoad){
		NetworkStart = startLoad;
	}
}
